namespace eShop.Ordering.API.Infrastructure.Services;

/// <summary>
/// Resolves the current request's user identity and display name from the ambient
/// <see cref="Microsoft.AspNetCore.Http.HttpContext" /> for <see cref="IIdentityService" />.
/// </summary>
public class IdentityService(IHttpContextAccessor context) : IIdentityService
{
    // IIdentityService mandates non-nullable string, but HttpContext/the "sub" claim/the
    // Identity's Name can genuinely be absent in general. Upstream has no guard here --
    // it assumes every call site runs within an authenticated request (enforced by
    // endpoint authorization requirements), matching that exact assumption.
    public string GetUserIdentity()
        => context.HttpContext?.User.FindFirst("sub")?.Value!;

    public string GetUserName()
        => context.HttpContext?.User.Identity?.Name!;
}
