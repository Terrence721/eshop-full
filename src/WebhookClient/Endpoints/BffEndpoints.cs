using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace eShop.WebhookClient.Endpoints;

// New endpoints a React frontend needs that upstream's Blazor Server app never did --
// Razor components called WebhooksClient/HooksRepository directly in C#, with no HTTP
// boundary, so there was nothing to log in to from the browser's own perspective.
public static class BffEndpoints
{
    public static IEndpointRouteBuilder MapBffEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/bff/login", Login).AllowAnonymous();
        app.MapGet("/bff/user", GetUser).AllowAnonymous();

        return app;
    }

    public static IResult Login(string? returnUrl)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = SanitizeReturnUrl(returnUrl)
        };

        return Results.Challenge(properties, [OpenIdConnectDefaults.AuthenticationScheme]);
    }

    public static IResult GetUser(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return Results.Unauthorized();
        }

        var claims = user.Claims.Select(c => new { c.Type, c.Value });
        return Results.Ok(claims);
    }

    // Prevent open redirects -- only ever redirect back into this same app.
    public static string SanitizeReturnUrl(string? returnUrl)
    {
        const string fallback = "/";

        if (string.IsNullOrEmpty(returnUrl))
        {
            return fallback;
        }

        if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            return new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }

        return returnUrl[0] == '/' ? returnUrl : $"{fallback}{returnUrl}";
    }
}
