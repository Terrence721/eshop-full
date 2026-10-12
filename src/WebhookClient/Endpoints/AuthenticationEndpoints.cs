using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace eShop.WebhookClient.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        // antiforgery.ValidateRequestAsync() dropped -- it's a server-rendered-form
        // mechanism (hidden field auto-injected by the <form> tag helper) with no clean
        // SPA equivalent without extra plumbing. Known, deliberate gap, same as every
        // converted Identity.API Quickstart action: revisit once this project's real
        // deployment topology (cookie domain, same-site vs. cross-site) is known.
        app.MapPost("/logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        });

        return app;
    }
}
