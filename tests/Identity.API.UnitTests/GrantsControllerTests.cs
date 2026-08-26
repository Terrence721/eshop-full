using System.Security.Claims;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace IdentityServerHost.Quickstart.UI.UnitTests;

[TestClass]
public class GrantsControllerTests
{
    private static GrantsController CreateController(
        IIdentityServerInteractionService interaction,
        IClientStore? clients = null,
        IResourceStore? resources = null,
        IEventService? events = null)
    {
        var controller = new GrantsController(
            interaction,
            clients ?? new InMemoryClientStore([]),
            resources ?? new InMemoryResourcesStore([], [], []),
            events ?? Substitute.For<IEventService>());

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
    public async Task Index_excludes_grants_for_clients_that_no_longer_exist()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAllUserGrantsAsync(Arg.Any<CancellationToken>()).Returns(
        [
            new Grant { ClientId = "still-exists", Scopes = ["openid"] },
            new Grant { ClientId = "removed-client", Scopes = ["openid"] }
        ]);
        var clients = new InMemoryClientStore([new Client { ClientId = "still-exists", ClientName = "Still Exists" }]);
        var controller = CreateController(interaction, clients: clients);

        var result = await controller.Index(CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as GrantsViewModel;
        var grant = vm!.Grants.Single();
        Assert.AreEqual("still-exists", grant.ClientId);
    }

    [TestMethod]
    public async Task Index_falls_back_to_ClientId_when_ClientName_is_null()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAllUserGrantsAsync(Arg.Any<CancellationToken>()).Returns(
            [new Grant { ClientId = "no-name-client", Scopes = ["openid"] }]);
        var clients = new InMemoryClientStore([new Client { ClientId = "no-name-client", ClientName = null }]);
        var controller = CreateController(interaction, clients: clients);

        var result = await controller.Index(CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as GrantsViewModel;
        Assert.AreEqual("no-name-client", vm!.Grants.Single().ClientName);
    }

    [TestMethod]
    public async Task Index_falls_back_to_scope_Name_when_DisplayName_is_null()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAllUserGrantsAsync(Arg.Any<CancellationToken>()).Returns(
            [new Grant { ClientId = "client-1", Scopes = ["orders"] }]);
        var clients = new InMemoryClientStore([new Client { ClientId = "client-1", ClientName = "Client 1" }]);
        var resources = new InMemoryResourcesStore([], [], [new ApiScope("orders") { DisplayName = null }]);
        var controller = CreateController(interaction, clients: clients, resources: resources);

        var result = await controller.Index(CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as GrantsViewModel;
        CollectionAssert.Contains(vm!.Grants.Single().ApiGrantNames.ToList(), "orders");
    }

    [TestMethod]
    public async Task Revoke_calls_RevokeUserConsentAsync_for_the_current_user_and_the_given_client()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetAllUserGrantsAsync(Arg.Any<CancellationToken>()).Returns([]);
        var events = Substitute.For<IEventService>();
        var controller = CreateController(interaction, events: events);

        await controller.Revoke("client-to-revoke", CancellationToken.None);

        await interaction.Received(1).RevokeUserConsentAsync("client-to-revoke", Arg.Any<CancellationToken>());
        await events.Received(1).RaiseAsync(
            Arg.Is<GrantsRevokedEvent>(e => e.ClientId == "client-to-revoke"),
            Arg.Any<CancellationToken>());
    }
}
