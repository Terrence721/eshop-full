using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using eShop.Identity.API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace IdentityServerHost.Quickstart.UI.UnitTests;

[TestClass]
public class AccountControllerTests
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

    private static AccountController CreateController(
        UserManager<ApplicationUser>? userManager = null,
        SignInManager<ApplicationUser>? signInManager = null,
        IIdentityServerInteractionService? interaction = null,
        IClientStore? clientStore = null,
        IAuthenticationSchemeProvider? schemeProvider = null,
        IAuthenticationHandlerProvider? handlerProvider = null,
        IEventService? events = null,
        ClaimsPrincipal? user = null)
    {
        userManager ??= CreateUserManager();
        var controller = new AccountController(
            userManager,
            signInManager ?? CreateSignInManager(userManager),
            interaction ?? Substitute.For<IIdentityServerInteractionService>(),
            clientStore ?? new InMemoryClientStore([]),
            schemeProvider ?? Substitute.For<IAuthenticationSchemeProvider>(),
            handlerProvider ?? Substitute.For<IAuthenticationHandlerProvider>(),
            events ?? Substitute.For<IEventService>());

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user ?? new ClaimsPrincipal(new ClaimsIdentity()) }
        };
        return controller;
    }

    // ---- Login GET ----

    [TestMethod]
    public async Task Get_Login_defaults_from_AccountOptions_when_no_authorization_context()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAuthorizationContextAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns((AuthorizationRequest?)null);
        var schemeProvider = Substitute.For<IAuthenticationSchemeProvider>();
        schemeProvider.GetAllSchemesAsync().Returns(new List<AuthenticationScheme>());
        var controller = CreateController(interaction: interaction, schemeProvider: schemeProvider);

        var result = await controller.Login((string?)null, CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as LoginViewModel;
        Assert.AreEqual(AccountOptions.AllowRememberLogin, vm!.AllowRememberLogin);
        Assert.IsTrue(vm.EnableLocalLogin);
    }

    [TestMethod]
    public async Task Get_Login_short_circuits_to_a_single_external_provider_when_context_has_a_registered_external_IdP()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAuthorizationContextAsync("return-url", Arg.Any<CancellationToken>())
            .Returns(new AuthorizationRequest { IdP = "google", LoginHint = "alice@example.com" });
        var schemeProvider = Substitute.For<IAuthenticationSchemeProvider>();
        schemeProvider.GetSchemeAsync("google").Returns(new AuthenticationScheme("google", "Google", typeof(IAuthenticationHandler)));
        var controller = CreateController(interaction: interaction, schemeProvider: schemeProvider);

        var result = await controller.Login("return-url", CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as LoginViewModel;
        Assert.IsFalse(vm!.EnableLocalLogin);
        Assert.AreEqual("google", vm.ExternalProviders.Single().AuthenticationScheme);
        Assert.AreEqual("alice@example.com", vm.Username);
    }

    [TestMethod]
    public async Task Get_Login_disables_local_login_when_the_matched_client_disables_it()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAuthorizationContextAsync("return-url", Arg.Any<CancellationToken>())
            .Returns(new AuthorizationRequest { Client = new Client { ClientId = "client-1" } });
        var schemeProvider = Substitute.For<IAuthenticationSchemeProvider>();
        schemeProvider.GetAllSchemesAsync().Returns(new List<AuthenticationScheme>());
        var clientStore = new InMemoryClientStore([new Client { ClientId = "client-1", Enabled = true, EnableLocalLogin = false }]);
        var controller = CreateController(interaction: interaction, schemeProvider: schemeProvider, clientStore: clientStore);

        var result = await controller.Login("return-url", CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as LoginViewModel;
        Assert.IsFalse(vm!.EnableLocalLogin);
    }

    // ---- Login POST ----

    [TestMethod]
    public async Task Post_Login_redirects_to_local_ReturnUrl_on_success_with_no_authorization_context()
    {
        var userManager = CreateUserManager();
        var user = new ApplicationUser { Id = "user-1", UserName = "alice" };
        userManager.FindByNameAsync("alice").Returns(user);
        var signInManager = CreateSignInManager(userManager);
        signInManager.PasswordSignInAsync("alice", "pw", false, true).Returns(SignInResult.Success);
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAuthorizationContextAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns((AuthorizationRequest?)null);
        var controller = CreateController(userManager: userManager, signInManager: signInManager, interaction: interaction);
        controller.Url = Substitute.For<IUrlHelper>();
        controller.Url.IsLocalUrl("/local/page").Returns(true);

        var result = await controller.Login(
            new LoginInputModel { Username = "alice", Password = "pw", ReturnUrl = "/local/page" },
            CancellationToken.None);

        var body = ((OkObjectResult)result.Result!).Value as LoginPostResult;
        Assert.AreEqual("/local/page", body!.RedirectUrl);
    }

    [TestMethod]
    public async Task Post_Login_redirects_home_when_ReturnUrl_is_empty()
    {
        var userManager = CreateUserManager();
        userManager.FindByNameAsync("alice").Returns(new ApplicationUser { Id = "user-1", UserName = "alice" });
        var signInManager = CreateSignInManager(userManager);
        signInManager.PasswordSignInAsync("alice", "pw", false, true).Returns(SignInResult.Success);
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAuthorizationContextAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns((AuthorizationRequest?)null);
        var controller = CreateController(userManager: userManager, signInManager: signInManager, interaction: interaction);
        // Url.IsLocalUrl is called unconditionally before the empty-ReturnUrl
        // check, even for a null/empty ReturnUrl - needs a real IUrlHelper here too.
        controller.Url = Substitute.For<IUrlHelper>();

        var result = await controller.Login(new LoginInputModel { Username = "alice", Password = "pw" }, CancellationToken.None);

        var body = ((OkObjectResult)result.Result!).Value as LoginPostResult;
        Assert.AreEqual("/", body!.RedirectUrl);
    }

    [TestMethod]
    public async Task Post_Login_throws_when_ReturnUrl_is_neither_local_nor_empty()
    {
        var userManager = CreateUserManager();
        userManager.FindByNameAsync("alice").Returns(new ApplicationUser { Id = "user-1", UserName = "alice" });
        var signInManager = CreateSignInManager(userManager);
        signInManager.PasswordSignInAsync("alice", "pw", false, true).Returns(SignInResult.Success);
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAuthorizationContextAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns((AuthorizationRequest?)null);
        var controller = CreateController(userManager: userManager, signInManager: signInManager, interaction: interaction);
        controller.Url = Substitute.For<IUrlHelper>();
        controller.Url.IsLocalUrl("https://evil.example").Returns(false);

        await Assert.ThrowsExactlyAsync<Exception>(() => controller.Login(
            new LoginInputModel { Username = "alice", Password = "pw", ReturnUrl = "https://evil.example" },
            CancellationToken.None));
    }

    [TestMethod]
    public async Task Post_Login_redisplays_with_an_error_on_invalid_credentials()
    {
        var userManager = CreateUserManager();
        var signInManager = CreateSignInManager(userManager);
        signInManager.PasswordSignInAsync("alice", "wrong", false, true).Returns(SignInResult.Failed);
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAuthorizationContextAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns((AuthorizationRequest?)null);
        var events = Substitute.For<IEventService>();
        var schemeProvider = Substitute.For<IAuthenticationSchemeProvider>();
        schemeProvider.GetAllSchemesAsync().Returns(new List<AuthenticationScheme>());
        var controller = CreateController(userManager: userManager, signInManager: signInManager, interaction: interaction, schemeProvider: schemeProvider, events: events);

        var result = await controller.Login(new LoginInputModel { Username = "alice", Password = "wrong" }, CancellationToken.None);

        var body = ((OkObjectResult)result.Result!).Value as LoginPostResult;
        Assert.IsNotNull(body!.ViewModel);
        await events.Received(1).RaiseAsync(Arg.Any<UserLoginFailureEvent>(), Arg.Any<CancellationToken>());
    }

    // ---- LoginCancel ----

    [TestMethod]
    public async Task LoginCancel_denies_and_redirects_when_context_exists()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        var request = new AuthorizationRequest { RedirectUri = "https://client.example/callback" };
        interaction.GetAuthorizationContextAsync("return-url", Arg.Any<CancellationToken>()).Returns(request);
        var controller = CreateController(interaction: interaction);

        var result = await controller.LoginCancel("return-url", CancellationToken.None);

        var body = ((OkObjectResult)result.Result!).Value as LoginPostResult;
        Assert.AreEqual("return-url", body!.RedirectUrl);
        await interaction.Received(1).DenyAuthorizationAsync(request, InteractionError.AccessDenied, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task LoginCancel_redirects_home_when_no_context()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAuthorizationContextAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns((AuthorizationRequest?)null);
        var controller = CreateController(interaction: interaction);

        var result = await controller.LoginCancel(null, CancellationToken.None);

        var body = ((OkObjectResult)result.Result!).Value as LoginPostResult;
        Assert.AreEqual("/", body!.RedirectUrl);
    }

    // ---- Logout GET ----

    [TestMethod]
    public async Task Get_Logout_hides_the_prompt_when_not_authenticated()
    {
        var controller = CreateController();

        var result = await controller.Logout((string?)null, CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as LogoutViewModel;
        Assert.IsFalse(vm!.ShowLogoutPrompt);
    }

    [TestMethod]
    public async Task Get_Logout_shows_the_prompt_when_interaction_context_says_so()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetLogoutContextAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(new LogoutRequest("iframe-url", null));
        var identity = new ClaimsIdentity([new Claim("sub", "user-1")], "auth-type");
        var controller = CreateController(interaction: interaction, user: new ClaimsPrincipal(identity));

        var result = await controller.Logout((string?)null, CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as LogoutViewModel;
        Assert.IsTrue(vm!.ShowLogoutPrompt);
    }

    // Get_Logout's "interaction says it's safe to auto-signout" branch
    // (LogoutRequest.ShowSignoutPrompt == false) isn't covered here -
    // ShowSignoutPrompt is derived internally by Duende and isn't settable
    // through any publicly constructible LogoutRequest/LogoutMessage path
    // (verified empirically: RequiresConfirmation true/false/message-null all
    // produced ShowSignoutPrompt == true). The "not authenticated" branch
    // above exercises the same vm.ShowLogoutPrompt = false assignment; the
    // test above exercises the ShowSignoutPrompt == true override instead,
    // since that's the value every real, constructible LogoutRequest produces.

    // ---- Logout POST ----

    [TestMethod]
    public async Task Post_Logout_signs_out_and_raises_the_success_event_when_authenticated()
    {
        var userManager = CreateUserManager();
        var signInManager = CreateSignInManager(userManager);
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetLogoutContextAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(new LogoutRequest("iframe-url", null));
        var events = Substitute.For<IEventService>();
        var identity = new ClaimsIdentity([new Claim("sub", "user-1")], "auth-type");
        var controller = CreateController(userManager: userManager, signInManager: signInManager, interaction: interaction, events: events, user: new ClaimsPrincipal(identity));

        var result = await controller.LogoutPost("logout-1", CancellationToken.None);

        await signInManager.Received(1).SignOutAsync();
        await events.Received(1).RaiseAsync(Arg.Any<UserLogoutSuccessEvent>(), Arg.Any<CancellationToken>());
        var redirect = (RedirectToActionResult)result;
        Assert.AreEqual(nameof(AccountController.LoggedOut), redirect.ActionName);
        Assert.AreEqual("logout-1", redirect.RouteValues!["logoutId"]);
    }

    // ---- LoggedOut GET ----

    [TestMethod]
    public async Task Get_LoggedOut_returns_the_view_model()
    {
        var controller = CreateController();

        var result = await controller.LoggedOut("logout-1", CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as LoggedOutViewModel;
        Assert.AreEqual("logout-1", vm!.LogoutId);
    }

    [TestMethod]
    public async Task AccessDenied_returns_NoContent()
    {
        var controller = CreateController();

        var result = controller.AccessDenied();

        Assert.IsInstanceOfType<NoContentResult>(result);
    }
}
