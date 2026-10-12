using System.Threading.Channels;

namespace eShop.WebhookClient.Endpoints;

// New endpoints a React frontend needs that upstream's Blazor Server app never did --
// Razor components called WebhooksClient/HooksRepository directly in C#, with no HTTP
// boundary, so there was nothing to expose as JSON or push over SSE.
public static class HooksApiEndpoints
{
    public static IEndpointRouteBuilder MapHooksApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/hooks", async (WebhooksClient client) =>
        {
            var hooks = await client.LoadWebhooks();
            return Results.Ok(hooks);
        });

        app.MapPost("/api/hooks", async (WebhookSubscriptionRequest request, WebhooksClient client) =>
        {
            var response = await client.AddWebHookAsync(request);
            return Results.StatusCode((int)response.StatusCode);
        });

        app.MapGet("/api/received", async (HooksRepository repository) =>
        {
            var hooks = await repository.GetAll();
            return Results.Ok(hooks);
        });

        // HooksRepository.Subscribe's callback is a pure "something changed" signal with
        // no payload (matches upstream exactly, unchanged) -- re-fetch GetAll() on each
        // notification and forward just the newest item into this connection's own channel.
        app.MapGet("/api/received/stream", (HooksRepository repository, CancellationToken cancellationToken) =>
        {
            var channel = Channel.CreateUnbounded<WebHookReceived>();

            var subscription = repository.Subscribe(async () =>
            {
                var hooks = await repository.GetAll();
                var latest = hooks.LastOrDefault();
                if (latest is not null)
                {
                    await channel.Writer.WriteAsync(latest, cancellationToken);
                }
            });

            cancellationToken.Register(() =>
            {
                subscription.Dispose();
                channel.Writer.TryComplete();
            });

            return TypedResults.ServerSentEvents(channel.Reader.ReadAllAsync(cancellationToken), eventType: "hook-received");
        });

        return app;
    }
}
