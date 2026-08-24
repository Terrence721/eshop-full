// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// This sample controller allows a user to revoke grants given to clients
/// </summary>
[ApiController]
[Route("[controller]/[action]")]
[SecurityHeaders]
[Authorize]
public class GrantsController : ControllerBase
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IClientStore _clients;
    private readonly IResourceStore _resources;
    private readonly IEventService _events;

    public GrantsController(IIdentityServerInteractionService interaction,
        IClientStore clients,
        IResourceStore resources,
        IEventService events)
    {
        _interaction = interaction;
        _clients = clients;
        _resources = resources;
        _events = events;
    }

    /// <summary>
    /// Show list of grants
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<GrantsViewModel>> Index(CancellationToken cancellationToken)
    {
        return Ok(await BuildViewModelAsync(cancellationToken));
    }

    /// <summary>
    /// Handle postback to revoke a client
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GrantsViewModel>> Revoke(string clientId, CancellationToken cancellationToken)
    {
        await _interaction.RevokeUserConsentAsync(clientId, cancellationToken);
        await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), clientId), cancellationToken);

        return Ok(await BuildViewModelAsync(cancellationToken));
    }

    private async Task<GrantsViewModel> BuildViewModelAsync(CancellationToken cancellationToken)
    {
        var grants = await _interaction.GetAllUserGrantsAsync(cancellationToken);

        var list = new List<GrantViewModel>();
        foreach (var grant in grants)
        {
            var client = await _clients.FindClientByIdAsync(grant.ClientId, cancellationToken);
            if (client != null)
            {
                var resources = await _resources.FindResourcesByScopeAsync(grant.Scopes, cancellationToken);

                var item = new GrantViewModel()
                {
                    ClientId = client.ClientId,
                    ClientName = client.ClientName ?? client.ClientId,
                    ClientLogoUrl = client.LogoUri,
                    ClientUrl = client.ClientUri,
                    Description = grant.Description,
                    Created = grant.CreationTime,
                    Expires = grant.Expiration,
                    IdentityGrantNames = resources.IdentityResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
                    ApiGrantNames = resources.ApiScopes.Select(x => x.DisplayName ?? x.Name).ToArray()
                };

                list.Add(item);
            }
        }

        return new GrantsViewModel
        {
            Grants = list
        };
    }
}
