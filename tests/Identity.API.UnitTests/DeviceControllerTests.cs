using System.Security.Claims;
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
public class DeviceControllerTests
{
    private static DeviceFlowAuthorizationRequest CreateRequest(Client client, Resources resources, IEnumerable<ParsedScopeValue>? parsedScopes = null) =>
        new()
        {
            Client = client,
            ValidatedResources = new ResourceValidationResult(resources, parsedScopes ?? [])
        };

    private static DeviceController CreateController(IDeviceFlowInteractionService interaction, IEventService? events = null)
    {
        var controller = new DeviceController(interaction, events ?? Substitute.For<IEventService>(), Substitute.For<ILogger<DeviceController>>());
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
    public async Task Get_Index_needs_user_code_when_none_given()
    {
        var controller = CreateController(Substitute.For<IDeviceFlowInteractionService>());

        var result = await controller.Index(null, CancellationToken.None);

        var body = ((OkObjectResult)result.Result!).Value as DeviceIndexResult;
        Assert.IsTrue(body!.NeedsUserCode);
    }

    [TestMethod]
    public async Task Get_Index_returns_NotFound_for_an_unknown_user_code()
    {
        var interaction = Substitute.For<IDeviceFlowInteractionService>();
        interaction.GetAuthorizationContextAsync("bad-code", Arg.Any<CancellationToken>()).Returns((DeviceFlowAuthorizationRequest?)null);
        var controller = CreateController(interaction);

        var result = await controller.Index("bad-code", CancellationToken.None);

        Assert.IsInstanceOfType<NotFoundResult>(result.Result);
    }

    [TestMethod]
    public async Task Get_Index_confirms_the_user_code_for_a_known_code()
    {
        var interaction = Substitute.For<IDeviceFlowInteractionService>();
        var request = CreateRequest(new Client { ClientId = "client-1" }, new Resources([], [], []));
        interaction.GetAuthorizationContextAsync("good-code", Arg.Any<CancellationToken>()).Returns(request);
        var controller = CreateController(interaction);

        var result = await controller.Index("good-code", CancellationToken.None);

        var body = ((OkObjectResult)result.Result!).Value as DeviceIndexResult;
        Assert.IsTrue(body!.ViewModel!.ConfirmUserCode);
    }

    [TestMethod]
    public async Task UserCodeCapture_returns_NotFound_for_an_unknown_user_code()
    {
        var interaction = Substitute.For<IDeviceFlowInteractionService>();
        interaction.GetAuthorizationContextAsync("bad-code", Arg.Any<CancellationToken>()).Returns((DeviceFlowAuthorizationRequest?)null);
        var controller = CreateController(interaction);

        var result = await controller.UserCodeCapture("bad-code", CancellationToken.None);

        Assert.IsInstanceOfType<NotFoundResult>(result.Result);
    }

    [TestMethod]
    public async Task Callback_returns_NotFound_when_no_matching_device_flow_authorization()
    {
        var interaction = Substitute.For<IDeviceFlowInteractionService>();
        interaction.GetAuthorizationContextAsync("code", Arg.Any<CancellationToken>()).Returns((DeviceFlowAuthorizationRequest?)null);
        var controller = CreateController(interaction);

        var result = await controller.Callback(new DeviceAuthorizationInputModel { UserCode = "code", Button = "yes" }, CancellationToken.None);

        Assert.IsInstanceOfType<NotFoundResult>(result.Result);
    }

    [TestMethod]
    public async Task Callback_denies_and_returns_NoContent_when_button_is_no()
    {
        var interaction = Substitute.For<IDeviceFlowInteractionService>();
        var request = CreateRequest(new Client { ClientId = "client-1" }, new Resources([], [], []));
        interaction.GetAuthorizationContextAsync("code", Arg.Any<CancellationToken>()).Returns(request);
        var events = Substitute.For<IEventService>();
        var controller = CreateController(interaction, events);

        var result = await controller.Callback(new DeviceAuthorizationInputModel { UserCode = "code", Button = "no" }, CancellationToken.None);

        Assert.IsInstanceOfType<NoContentResult>(result.Result);
        await interaction.Received(1).HandleRequestAsync(
            "code",
            Arg.Is<ConsentResponse>(c => c.Error == InteractionError.AccessDenied),
            Arg.Any<CancellationToken>());
        await events.Received(1).RaiseAsync(Arg.Any<ConsentDeniedEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Callback_grants_and_returns_NoContent_when_button_is_yes_with_scopes_consented()
    {
        var interaction = Substitute.For<IDeviceFlowInteractionService>();
        var request = CreateRequest(new Client { ClientId = "client-1" }, new Resources([], [], []));
        interaction.GetAuthorizationContextAsync("code", Arg.Any<CancellationToken>()).Returns(request);
        var events = Substitute.For<IEventService>();
        var controller = CreateController(interaction, events);
        var model = new DeviceAuthorizationInputModel { UserCode = "code", Button = "yes", ScopesConsented = ["profile"] };

        var result = await controller.Callback(model, CancellationToken.None);

        Assert.IsInstanceOfType<NoContentResult>(result.Result);
        await interaction.Received(1).HandleRequestAsync(
            "code",
            Arg.Is<ConsentResponse>(c => c.ScopesValuesConsented!.Single() == "profile"),
            Arg.Any<CancellationToken>());
        await events.Received(1).RaiseAsync(Arg.Any<ConsentGrantedEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Callback_redisplays_with_the_real_error_message_when_button_is_yes_but_no_scopes_consented()
    {
        var interaction = Substitute.For<IDeviceFlowInteractionService>();
        var request = CreateRequest(new Client { ClientId = "client-1" }, new Resources([], [], []));
        interaction.GetAuthorizationContextAsync("code", Arg.Any<CancellationToken>()).Returns(request);
        var controller = CreateController(interaction);
        var model = new DeviceAuthorizationInputModel { UserCode = "code", Button = "yes", ScopesConsented = [] };

        var result = await controller.Callback(model, CancellationToken.None);

        var body = ((OkObjectResult)result.Result!).Value as DeviceCallbackResult;
        Assert.AreEqual(ConsentOptions.MustChooseOneErrorMessage, body!.ValidationError);
        Assert.IsNotNull(body.ViewModel);
    }

    [TestMethod]
    public async Task Callback_redisplays_with_the_real_error_message_when_button_is_neither_yes_nor_no()
    {
        var interaction = Substitute.For<IDeviceFlowInteractionService>();
        var request = CreateRequest(new Client { ClientId = "client-1" }, new Resources([], [], []));
        interaction.GetAuthorizationContextAsync("code", Arg.Any<CancellationToken>()).Returns(request);
        var controller = CreateController(interaction);
        var model = new DeviceAuthorizationInputModel { UserCode = "code", Button = "cancel" };

        var result = await controller.Callback(model, CancellationToken.None);

        var body = ((OkObjectResult)result.Result!).Value as DeviceCallbackResult;
        Assert.AreEqual(ConsentOptions.InvalidSelectionErrorMessage, body!.ValidationError);
    }
}
