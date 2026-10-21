/// <summary>
/// Extension methods that extract the authenticated user identity (gRPC to ASP.NET Core claims)
/// from a <see cref="ServerCallContext"/>. Both lookups tolerate missing claims and return
/// <see langword="null" /> instead of throwing.
/// </summary>
namespace eShop.Basket.API.Extensions;

internal static class ServerCallContextIdentityExtensions
{
    public static string? GetUserIdentity(this ServerCallContext context) => context.GetHttpContext().User.FindFirst("sub")?.Value;
    public static string? GetUserName(this ServerCallContext context) => context.GetHttpContext().User.FindFirst(x => x.Type == ClaimTypes.Name)?.Value;
}
