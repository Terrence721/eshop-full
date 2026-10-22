namespace eShop.Ordering.API.Infrastructure.Services
{
    /// <summary>
    /// Resolves the current request's user identity and display name from the ambient
    /// <see cref="Microsoft.AspNetCore.Http.HttpContext" />. Implemented by <see cref="IdentityService"/>.
    /// </summary>
    public interface IIdentityService
    {
        /// <summary>Gets the "sub" claim value identifying the current (authenticated) user.</summary>
        string GetUserIdentity();

        /// <summary>Gets the display name of the current (authenticated) user.</summary>
        string GetUserName();
    }
}
