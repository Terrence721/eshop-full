using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace eShop.WebhookClient.Endpoints;

// New endpoints a React frontend needs that upstream's Blazor Server app never did --
// Razor components called WebhooksClient/HooksRepository directly in C#, with no HTTP
// boundary, so there was nothing to expose as JSON or push over SSE.
/// <summary>
/// HTTP endpoints exposing hook registration and received-webhook data to the React SPA.
/// </summary>
public static class HooksApiEndpoints
{
    /// <summary>
    /// Maps hooks API and received-webhook streaming endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapHooksApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/hooks", GetHooksAsync);
        app.MapGet("/api/hooks/default-token", GetDefaultToken);
        app.MapPost("/api/hooks", RegisterHookAsync);
        app.MapGet("/api/received", GetReceivedAsync);
        app.MapGet("/api/received/stream", StreamReceived);

        return app;
    }

    /// <summary>
    /// Loads registered hooks from Webhooks.API.
    /// </summary>
    public static async Task<IResult> GetHooksAsync(WebhooksClient client)
    {
        var hooks = await client.LoadWebhooks();
        return Results.Ok(hooks);
    }

    /// <summary>
    /// Returns the shared default token used by AddWebhook.razor.
    /// </summary>
    // AddWebhook.razor pre-fills its token field from this same
    // IOptions<WebhookClientOptions>.Token -- not just a convenience
    // default, since it's the shared secret Webhooks.API validates a
    // registration's token against, so the SPA needs it over HTTP too.
    public static IResult GetDefaultToken(IOptions<WebhookClientOptions> options) =>
        Results.Ok(new { token = options.Value.Token });

    /// <summary>
    /// Registers a hook against Webhooks.API using server-derived URLs.
    /// </summary>
    public static async Task<IResult> RegisterHookAsync(RegisterHookRequest request, WebhooksClient client, IOptions<WebhookClientOptions> options, HttpContext httpContext)
    {
        var baseUrl = BuildBaseUrl(options.Value.SelfUrl, httpContext.Request.Scheme, httpContext.Request.Host.ToString());

        var payload = new WebhookSubscriptionRequest
        {
            Event = "OrderPaid",
            GrantUrl = $"{baseUrl}check",
            Url = $"{baseUrl}webhook-received",
            Token = request.Token,
        };

        var response = await client.AddWebHookAsync(payload);
        return Results.StatusCode((int)response.StatusCode);
    }

    /// <summary>
    /// Builds a base URL from configured SelfUrl or the current request.
    /// </summary>
    // AddWebhook.razor built the GrantUrl/Url from Nav.BaseUri -- the browser's own origin,
    // which was safe when the Blazor Server app and its endpoints shared one process. With a
    // separate SPA origin (the Vite dev server, and no same-origin static-file serving set up
    // for production either -- see Identity.API, which never wires that up for
    // Identity.WebApp), that base has to be WebhookClient's own address instead, built here
    // rather than trusted from the request.
    public static string BuildBaseUrl(string? selfUrl, string requestScheme, string requestHost) =>
        !string.IsNullOrEmpty(selfUrl) ? selfUrl : $"{requestScheme}://{requestHost}/";

    /// <summary>
    /// Returns received webhook items.
    /// </summary>
    public static async Task<IResult> GetReceivedAsync(HooksRepository repository)
    {
        var hooks = await repository.GetAll();
        return Results.Ok(hooks);
    }

    /// <summary>
    /// Streams newly received webhooks over SSE.
    /// </summary>
    // HooksRepository.Subscribe's callback is a pure "something changed" signal with no
    // payload (matches upstream exactly, unchanged) -- re-fetch GetAll() on each notification
    // and forward just the newest item into this connection's own channel. Not unit tested:
    // it's wired directly to SSE/Channel infrastructure and a live CancellationToken, which is
    // integration-test territory, not a single pure unit.
    public static IResult StreamReceived(HooksRepository repository, CancellationToken cancellationToken)
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
    }
}

// The SPA only ever supplies the token -- Event/GrantUrl/Url are fixed or
// server-derived, so WebhookSubscriptionRequest itself (which still needs
// those) never crosses the HTTP boundary to the frontend.
/// <summary>
/// Request from the SPA containing only the subscriber token.
/// </summary>
public record RegisterHookRequest(string Token);