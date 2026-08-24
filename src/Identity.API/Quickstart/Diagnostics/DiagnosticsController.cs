// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Buffers.Text;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServerHost.Quickstart.UI;

[ApiController]
[Route("[controller]/[action]")]
[SecurityHeaders]
[Authorize]
public class DiagnosticsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DiagnosticsViewModel>> Index()
    {
        var remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var localAddresses = new[] { "127.0.0.1", "::1", HttpContext.Connection.LocalIpAddress?.ToString() };
        if (remoteIpAddress is null || !localAddresses.Contains(remoteIpAddress))
        {
            return NotFound();
        }

        var result = await HttpContext.AuthenticateAsync();
        var properties = result.Properties?.Items ?? new Dictionary<string, string?>();

        var clients = Enumerable.Empty<string>();
        if (properties.TryGetValue("client_list", out var encoded) && encoded is not null)
        {
            var bytes = Base64Url.DecodeFromChars(encoded);
            var value = Encoding.UTF8.GetString(bytes);
            clients = JsonSerializer.Deserialize<string[]>(value) ?? [];
        }

        var model = new DiagnosticsViewModel
        {
            Claims = (result.Principal?.Claims ?? []).Select(c => new ClaimViewModel { Type = c.Type, Value = c.Value }),
            Properties = properties,
            Clients = clients
        };

        return Ok(model);
    }
}
