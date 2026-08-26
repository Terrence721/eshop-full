using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace IdentityServerHost.Quickstart.UI.UnitTests;

[TestClass]
public class ConsentControllerTests
{
    // RedirectUri is always populated on a real AuthorizationRequest by the time
    // it reaches consent processing - OIDC requires it, and Duende's own request
    // validation rejects a missing one long before this point. Set here so
    // IsNativeClient() (dereferences it unconditionally) doesn't NRE on a fixture
    // that skips a field production code never actually leaves null.
    private static AuthorizationRequest CreateRequest(Client client, Resources resources, IEnumerable<ParsedScopeValue>? parsedScopes = null) =>
        new()
        {
            Client = client,
            RedirectUri = "https://client.example/callback",
            ValidatedResources = new ResourceValidationResult(resources, parsedScopes ?? [])
        };

    private static ConsentController CreateController(IIdentityServerInteractionService interaction, IEventService? events = null)
    {
        var controller = new ConsentController(interaction, events ?? Substitute.For<IEventService>(), Substitute.For<ILogger<ConsentController>>());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "user-1")]))
            }
        };
        return controller;
    }

    [TestMethod]
    public async Task Get_Index_returns_NotFound_when_no_authorization_context()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAuthorizationContextAsync("return-url", Arg.Any<CancellationToken>()).Returns((AuthorizationRequest?)null);
        var controller = CreateController(interaction);

        var result = await controller.Index("return-url", CancellationToken.None);

        Assert.IsInstanceOfType<NotFoundResult>(result.Result);
    }

    [TestMethod]
    public async Task Get_Index_checks_every_scope_by_default_on_first_visit()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        var client = new Client { ClientId = "client-1", ClientName = "Client 1" };
        var resources = new Resources([new IdentityResource("profile", ["name"])], [], []);
        var request = CreateRequest(client, resources);
        interaction.GetAuthorizationContextAsync("return-url", Arg.Any<CancellationToken>()).Returns(request);
        var controller = CreateController(interaction);

        var result = await controller.Index("return-url", CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as ConsentViewModel;
        Assert.IsTrue(vm!.IdentityScopes.Single().Checked);
    }

    [TestMethod]
    public async Task Get_Index_falls_back_to_ClientId_when_ClientName_is_null()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        var client = new Client { ClientId = "client-1", ClientName = null };
        var request = CreateRequest(client, new Resources([], [], []));
        interaction.GetAuthorizationContextAsync("return-url", Arg.Any<CancellationToken>()).Returns(request);
        var controller = CreateController(interaction);

        var result = await controller.Index("return-url", CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as ConsentViewModel;
        Assert.AreEqual("client-1", vm!.ClientName);
    }

    [TestMethod]
    public async Task Get_Index_includes_offline_access_scope_when_resources_allow_it()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        var client = new Client { ClientId = "client-1" };
        var resources = new Resources([], [], []) { OfflineAccess = true };
        var request = CreateRequest(client, resources);
        interaction.GetAuthorizationContextAsync("return-url", Arg.Any<CancellationToken>()).Returns(request);
        var controller = CreateController(interaction);

        var result = await controller.Index("return-url", CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as ConsentViewModel;
        Assert.IsTrue(vm!.ApiScopes.Any(s => s.Value == IdentityServerConstants.StandardScopes.OfflineAccess));
    }

    [TestMethod]
    public async Task Post_Index_returns_NotFound_when_no_authorization_context()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAuthorizationContextAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns((AuthorizationRequest?)null);
        var controller = CreateController(interaction);

        var result = await controller.Index(new ConsentInputModel { Button = "yes", ReturnUrl = "return-url" }, CancellationToken.None);

        Assert.IsInstanceOfType<NotFoundResult>(result.Result);
    }

    [TestMethod]
    public async Task Post_Index_denies_and_redirects_when_button_is_no()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        var client = new Client { ClientId = "client-1" };
        var request = CreateRequest(client, new Resources([], [], []));
        interaction.GetAuthorizationContextAsync("return-url", Arg.Any<CancellationToken>()).Returns(request);
        var events = Substitute.For<IEventService>();
        var controller = CreateController(interaction, events);

        var result = await controller.Index(new ConsentInputModel { Button = "no", ReturnUrl = "return-url" }, CancellationToken.None);

        var body = ((OkObjectResult)result.Result!).Value as ConsentPostResult;
        Assert.AreEqual("return-url", body!.RedirectUrl);
        await interaction.Received(1).GrantConsentAsync(
            request,
            Arg.Is<ConsentResponse>(c => c.Error == InteractionError.AccessDenied),
            Arg.Any<CancellationToken>());
        await events.Received(1).RaiseAsync(Arg.Any<ConsentDeniedEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Post_Index_grants_and_redirects_when_button_is_yes_with_scopes_consented()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        var client = new Client { ClientId = "client-1" };
        var request = CreateRequest(client, new Resources([], [], []));
        interaction.GetAuthorizationContextAsync("return-url", Arg.Any<CancellationToken>()).Returns(request);
        var events = Substitute.For<IEventService>();
        var controller = CreateController(interaction, events);
        var model = new ConsentInputModel { Button = "yes", ReturnUrl = "return-url", ScopesConsented = ["profile"] };

        var result = await controller.Index(model, CancellationToken.None);

        var body = ((OkObjectResult)result.Result!).Value as ConsentPostResult;
        Assert.AreEqual("return-url", body!.RedirectUrl);
        await interaction.Received(1).GrantConsentAsync(
            request,
            Arg.Is<ConsentResponse>(c => c.ScopesValuesConsented!.Single() == "profile"),
            Arg.Any<CancellationToken>());
        await events.Received(1).RaiseAsync(Arg.Any<ConsentGrantedEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Post_Index_redisplays_with_error_when_button_is_yes_but_no_scopes_consented()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        var client = new Client { ClientId = "client-1" };
        var request = CreateRequest(client, new Resources([], [], []));
        interaction.GetAuthorizationContextAsync("return-url", Arg.Any<CancellationToken>()).Returns(request);
        var controller = CreateController(interaction);
        var model = new ConsentInputModel { Button = "yes", ReturnUrl = "return-url", ScopesConsented = [] };

        var result = await controller.Index(model, CancellationToken.None);

        var body = ((OkObjectResult)result.Result!).Value as ConsentPostResult;
        Assert.AreEqual(ConsentOptions.MustChooseOneErrorMessage, body!.ValidationError);
        Assert.IsNotNull(body.ViewModel);
    }

    [TestMethod]
    public async Task Post_Index_redisplays_with_error_when_button_is_neither_yes_nor_no()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        var client = new Client { ClientId = "client-1" };
        var request = CreateRequest(client, new Resources([], [], []));
        interaction.GetAuthorizationContextAsync("return-url", Arg.Any<CancellationToken>()).Returns(request);
        var controller = CreateController(interaction);
        var model = new ConsentInputModel { Button = "cancel", ReturnUrl = "return-url" };

        var result = await controller.Index(model, CancellationToken.None);

        var body = ((OkObjectResult)result.Result!).Value as ConsentPostResult;
        Assert.AreEqual(ConsentOptions.InvalidSelectionErrorMessage, body!.ValidationError);
    }
}
