using System.Security.Claims;
using System.Threading.Channels;
using Duende.Bff;
using eShop.WebBFF.Services;

namespace eShop.WebBFF.Endpoints;

public static class OrderStatusEndpoints
{
    public static IEndpointRouteBuilder MapOrderStatusEndpoints(this IEndpointRouteBuilder app)
    {
        // No payload: every one of the 6 real order-status integration events just means
        // "this buyer's orders changed" (matches upstream's own OrderStatusNotificationService
        // exactly -- it never inspects event details either). The frontend re-fetches
        // /remoteapi/orders on receipt rather than being handed order data over this channel.
        app.MapGet("/api/orders/stream", (OrderStatusNotifications notifications, ClaimsPrincipal user, CancellationToken cancellationToken) =>
        {
            var buyerIdentityGuid = user.FindFirst("sub")!.Value;
            var channel = Channel.CreateUnbounded<string>();

            var subscription = notifications.Subscribe(buyerIdentityGuid, async () =>
            {
                await channel.Writer.WriteAsync("changed", cancellationToken);
            });

            cancellationToken.Register(() =>
            {
                subscription.Dispose();
                channel.Writer.TryComplete();
            });

            return TypedResults.ServerSentEvents(channel.Reader.ReadAllAsync(cancellationToken), eventType: "order-status-changed");
        })
        .RequireAuthorization()
        .AsBffApiEndpoint();

        return app;
    }
}
