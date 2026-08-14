using System.Security.Claims;

namespace eShop.ServiceDefaults;

public static class ClaimsPrincipalExtensions
{
    // Falls back to the long-form claim in case AuthenticationExtensions' "sub" remap removal didn't run.
    public static string? GetUserId(this ClaimsPrincipal principal)
        => principal.FindFirst("sub")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public static string? GetUserName(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.Name)?.Value;
}
