using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using NSubstitute;

namespace IdentityServerHost.Quickstart.UI.UnitTests;

[TestClass]
public class ConsentResponseBuilderTests
{
    [TestMethod]
    public async Task BuildAsync_denies_and_raises_ConsentDeniedEvent_when_button_is_no()
    {
        var events = Substitute.For<IEventService>();
        var builder = new ConsentResponseBuilder(events);
        var model = new ConsentInputModel { Button = "no" };

        var (grantedConsent, validationError) = await builder.BuildAsync(model, "sub-1", "client-1", ["openid"], CancellationToken.None);

        Assert.IsNull(validationError);
        Assert.AreEqual(InteractionError.AccessDenied, grantedConsent!.Error);
        await events.Received(1).RaiseAsync(Arg.Any<ConsentDeniedEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task BuildAsync_grants_and_raises_ConsentGrantedEvent_when_button_is_yes_with_scopes()
    {
        var events = Substitute.For<IEventService>();
        var builder = new ConsentResponseBuilder(events);
        var model = new ConsentInputModel { Button = "yes", ScopesConsented = ["profile"], RememberConsent = true };

        var (grantedConsent, validationError) = await builder.BuildAsync(model, "sub-1", "client-1", ["openid"], CancellationToken.None);

        Assert.IsNull(validationError);
        Assert.AreEqual("profile", grantedConsent!.ScopesValuesConsented!.Single());
        Assert.IsTrue(grantedConsent.RememberConsent);
        await events.Received(1).RaiseAsync(Arg.Any<ConsentGrantedEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task BuildAsync_returns_MustChooseOneErrorMessage_when_button_is_yes_with_no_scopes()
    {
        var events = Substitute.For<IEventService>();
        var builder = new ConsentResponseBuilder(events);
        var model = new ConsentInputModel { Button = "yes", ScopesConsented = [] };

        var (grantedConsent, validationError) = await builder.BuildAsync(model, "sub-1", "client-1", ["openid"], CancellationToken.None);

        Assert.IsNull(grantedConsent);
        Assert.AreEqual(ConsentOptions.MustChooseOneErrorMessage, validationError);
        await events.DidNotReceiveWithAnyArgs().RaiseAsync(default!, default);
    }

    [TestMethod]
    public async Task BuildAsync_returns_InvalidSelectionErrorMessage_for_an_unrecognized_button()
    {
        var events = Substitute.For<IEventService>();
        var builder = new ConsentResponseBuilder(events);
        var model = new ConsentInputModel { Button = "cancel" };

        var (grantedConsent, validationError) = await builder.BuildAsync(model, "sub-1", "client-1", ["openid"], CancellationToken.None);

        Assert.IsNull(grantedConsent);
        Assert.AreEqual(ConsentOptions.InvalidSelectionErrorMessage, validationError);
        await events.DidNotReceiveWithAnyArgs().RaiseAsync(default!, default);
    }
}
