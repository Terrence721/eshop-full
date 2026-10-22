using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace eShop.WebhookClient.Endpoints
{
    // New endpoints a React frontend needs that upstream's Blazor Server app never did --
    // Razor components called WebhooksClient/HooksRepository directly in C#, with no HTTP
    // boundary, so there was nothing to log in to from the browser's own perspective.
    /// <summary>
    /// BFF-style login/user endpoints for the React frontend.
    /// </summary>
    public static class BffEndpoints
    {
        /// <summary>
        /// Maps the BFF login and user endpoints.
        /// </summary>
        public static IEndpointRouteBuilder MapBffEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/bff/login", Login).AllowAnonymous();
            app.MapGet("/bff/user", GetUser).AllowAnonymous();

            return app;
        }

        /// <summary>
        /// Challenges the OIDC scheme and redirects to the sanitized return URL.
        /// </summary>
        public static IResult Login(string? returnUrl)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = SanitizeReturnUrl(returnUrl)
            };

            return Results.Challenge(properties, [OpenIdConnectDefaults.AuthenticationScheme]);
        }

        /// <summary>
        /// Returns the current authenticated user's claims, or 401 when anonymous.
        /// </summary>
        public static IResult GetUser(ClaimsPrincipal user)
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var claims = user.Claims.Select(c => new { c.Type, c.Value });
            return Results.Ok(claims);
        }

        /// <summary>
        /// Prevents open redirects by ensuring the return URL resolves inside this app.
        /// </summary>
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
}