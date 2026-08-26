using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using eShop.Identity.API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace IdentityServerHost.Quickstart.UI.UnitTests;

[TestClass]
public class ExternalControllerTests
{
    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(store, null, null, null, null, null, null, null, null);
    }

    private static SignInManager<ApplicationUser> CreateSignInManager(UserManager<ApplicationUser> userManager)
    {
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        contextAccessor.HttpContext.Returns(new DefaultHttpContext());
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>();
        return Substitute.For<SignInManager<ApplicationUser>>(userManager, contextAccessor, claimsFactory, null, null, null, null);
    }

    private static ExternalController CreateController(
        UserManager<ApplicationUser>? userManager = null,
        SignInManager<ApplicationUser>? signInManager = null,
        IIdentityServerInteractionService? interaction = null,
        IEventService? events = null,
        IAuthenticationService? authService = null)
    {
        userManager ??= CreateUserManager();
        var controller = new ExternalController(
            userManager,
            signInManager ?? CreateSignInManager(userManager),
            interaction ?? Substitute.For<IIdentityServerInteractionService>(),
            events ?? Substitute.For<IEventService>(),
            Substitute.For<ILogger<ExternalController>>());

        var schemeProvider = Substitute.For<IAuthenticationSchemeProvider>();
        schemeProvider.GetDefaultAuthenticateSchemeAsync().Returns(
            new AuthenticationScheme("cookie", null, typeof(CookieAuthenticationHandler)));

        var services = new ServiceCollection();
        services.AddSingleton(authService ?? Substitute.For<IAuthenticationService>());
        services.AddSingleton(new IdentityServerOptions());
        services.AddSingleton(schemeProvider);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() }
        };
        return controller;
    }

    // ---- Challenge ----

    [TestMethod]
    public void Challenge_throws_when_returnUrl_is_neither_local_nor_a_valid_OIDC_url()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.IsValidReturnUrl("https://evil.example").Returns(false);
        var controller = CreateController(interaction: interaction);
        controller.Url = Substitute.For<IUrlHelper>();
        controller.Url.IsLocalUrl("https://evil.example").Returns(false);

        Assert.ThrowsExactly<Exception>(() => controller.Challenge("google", "https://evil.example"));
    }

    [TestMethod]
    public void Challenge_defaults_empty_returnUrl_to_home_and_succeeds()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        var controller = CreateController(interaction: interaction);
        controller.Url = Substitute.For<IUrlHelper>();
        controller.Url.IsLocalUrl("~/").Returns(true);
        controller.Url.Action(Arg.Any<UrlActionContext>()).Returns("/External/Callback");

        var result = controller.Challenge("google", null);

        var challenge = (ChallengeResult)result;
        Assert.AreEqual("google", challenge.AuthenticationSchemes.Single());
        Assert.AreEqual("~/", challenge.Properties!.Items["returnUrl"]);
        Assert.AreEqual("google", challenge.Properties.Items["scheme"]);
        Assert.AreEqual("/External/Callback", challenge.Properties.RedirectUri);
    }

    // ---- Callback ----

    private static AuthenticateResult SuccessResult(ClaimsPrincipal principal, AuthenticationProperties properties) =>
        AuthenticateResult.Success(new AuthenticationTicket(principal, properties, "external"));

    private static ClaimsPrincipal ExternalPrincipal(string subjectId, params Claim[] extraClaims) =>
        new(new ClaimsIdentity([new Claim(JwtClaimTypes.Subject, subjectId), .. extraClaims]));

    private static AuthenticationProperties PropertiesWith(string returnUrl = "/", string scheme = "google") =>
        new(new Dictionary<string, string?> { ["returnUrl"] = returnUrl, ["scheme"] = scheme });

    [TestMethod]
    public async Task Callback_throws_when_external_authentication_did_not_succeed()
    {
        var authService = Substitute.For<IAuthenticationService>();
        authService.AuthenticateAsync(Arg.Any<HttpContext>(), IdentityServerConstants.ExternalCookieAuthenticationScheme)
            .Returns(AuthenticateResult.NoResult());
        var controller = CreateController(authService: authService);

        await Assert.ThrowsExactlyAsync<Exception>(() => controller.Callback(CancellationToken.None));
    }

    [TestMethod]
    public async Task Callback_throws_when_scheme_item_is_missing()
    {
        var principal = ExternalPrincipal("ext-1");
        var properties = new AuthenticationProperties(new Dictionary<string, string?> { ["returnUrl"] = "/" });
        var authService = Substitute.For<IAuthenticationService>();
        authService.AuthenticateAsync(Arg.Any<HttpContext>(), IdentityServerConstants.ExternalCookieAuthenticationScheme)
            .Returns(SuccessResult(principal, properties));
        var controller = CreateController(authService: authService);

        await Assert.ThrowsExactlyAsync<Exception>(() => controller.Callback(CancellationToken.None));
    }

    [TestMethod]
    public async Task Callback_auto_provisions_a_new_user_when_no_existing_login_is_found()
    {
        var principal = ExternalPrincipal("ext-1", new Claim(JwtClaimTypes.Name, "Alice External"));
        var properties = PropertiesWith();
        var authService = Substitute.For<IAuthenticationService>();
        authService.AuthenticateAsync(Arg.Any<HttpContext>(), IdentityServerConstants.ExternalCookieAuthenticationScheme)
            .Returns(SuccessResult(principal, properties));

        var userManager = CreateUserManager();
        userManager.FindByLoginAsync("google", "ext-1").Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
        userManager.AddClaimsAsync(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<Claim>>()).Returns(IdentityResult.Success);
        userManager.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>()).Returns(IdentityResult.Success);

        var signInManager = CreateSignInManager(userManager);
        signInManager.CreateUserPrincipalAsync(Arg.Any<ApplicationUser>())
            .Returns(new ClaimsPrincipal(new ClaimsIdentity([new Claim(JwtClaimTypes.Name, "Alice External")])));

        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAuthorizationContextAsync("/", Arg.Any<CancellationToken>()).Returns((AuthorizationRequest?)null);

        var controller = CreateController(userManager: userManager, signInManager: signInManager, interaction: interaction, authService: authService);

        var result = await controller.Callback(CancellationToken.None);

        Assert.IsInstanceOfType<RedirectResult>(result);
        Assert.AreEqual("/", ((RedirectResult)result).Url);
        await userManager.Received(1).CreateAsync(Arg.Any<ApplicationUser>());
        await userManager.Received(1).AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Is<UserLoginInfo>(l => l.LoginProvider == "google" && l.ProviderKey == "ext-1"));
    }

    [TestMethod]
    public async Task Callback_does_not_provision_when_a_matching_login_already_exists()
    {
        var principal = ExternalPrincipal("ext-1");
        var properties = PropertiesWith();
        var authService = Substitute.For<IAuthenticationService>();
        authService.AuthenticateAsync(Arg.Any<HttpContext>(), IdentityServerConstants.ExternalCookieAuthenticationScheme)
            .Returns(SuccessResult(principal, properties));

        var userManager = CreateUserManager();
        var existingUser = new ApplicationUser { Id = "user-1", UserName = "alice" };
        userManager.FindByLoginAsync("google", "ext-1").Returns(existingUser);

        var signInManager = CreateSignInManager(userManager);
        signInManager.CreateUserPrincipalAsync(existingUser).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAuthorizationContextAsync("/", Arg.Any<CancellationToken>()).Returns((AuthorizationRequest?)null);

        var controller = CreateController(userManager: userManager, signInManager: signInManager, interaction: interaction, authService: authService);

        await controller.Callback(CancellationToken.None);

        await userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>());
    }

    [TestMethod]
    public async Task Callback_raises_UserLoginSuccessEvent_and_redirects_to_the_real_returnUrl()
    {
        var principal = ExternalPrincipal("ext-1");
        var properties = PropertiesWith(returnUrl: "/dashboard");
        var authService = Substitute.For<IAuthenticationService>();
        authService.AuthenticateAsync(Arg.Any<HttpContext>(), IdentityServerConstants.ExternalCookieAuthenticationScheme)
            .Returns(SuccessResult(principal, properties));

        var userManager = CreateUserManager();
        var existingUser = new ApplicationUser { Id = "user-1", UserName = "alice" };
        userManager.FindByLoginAsync("google", "ext-1").Returns(existingUser);
        var signInManager = CreateSignInManager(userManager);
        signInManager.CreateUserPrincipalAsync(existingUser).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAuthorizationContextAsync("/dashboard", Arg.Any<CancellationToken>()).Returns((AuthorizationRequest?)null);
        var events = Substitute.For<IEventService>();

        var controller = CreateController(userManager: userManager, signInManager: signInManager, interaction: interaction, events: events, authService: authService);

        var result = await controller.Callback(CancellationToken.None);

        Assert.AreEqual("/dashboard", ((RedirectResult)result).Url);
        await events.Received(1).RaiseAsync(
            Arg.Is<UserLoginSuccessEvent>(e => e.ProviderUserId == "ext-1" && e.SubjectId == "user-1"),
            Arg.Any<CancellationToken>());
    }
}
