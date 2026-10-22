using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Webhooks.API.Extensions;

namespace Webhooks.API;

/// <summary>
/// Minimal API endpoints for managing webhook subscriptions.
/// </summary>
public static class WebHooksApi
{
    /// <summary>
    /// Registers the V1 webhook subscription endpoints.
    /// </summary>
    public static RouteGroupBuilder MapWebHooksApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/webhooks").HasApiVersion(1.0);

        api.MapGet("/", GetAllAsync);
        api.MapGet("/{id:int}", GetByIdAsync);
        api.MapPost("/", CreateAsync).ValidateWebhookSubscriptionRequest();
        api.MapDelete("/{id:int}", DeleteAsync);

        return api;
    }

    /// <summary>
    /// Returns all webhook subscriptions owned by the current user.
    /// </summary>
    public static async Task<Ok<List<WebhookSubscription>>> GetAllAsync(WebhooksContext context, ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        var data = await context.Subscriptions.Where(s => s.UserId == userId).ToListAsync();
        return TypedResults.Ok(data);
    }

    /// <summary>
    /// Returns a single webhook subscription by id, scoped to the current user.
    /// </summary>
    public static async Task<Results<Ok<WebhookSubscription>, NotFound<string>>> GetByIdAsync(
        WebhooksContext context,
        ClaimsPrincipal user,
        int id)
    {
        var userId = user.GetUserId();
        var subscription = await context.Subscriptions
            .SingleOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        if (subscription != null)
        {
            return TypedResults.Ok(subscription);
        }
        return TypedResults.NotFound($"Subscriptions {id} not found");
    }

    /// <summary>
    /// Creates a new webhook subscription after validating the grant URL.
    /// </summary>
    public static async Task<Results<Created, BadRequest<string>>> CreateAsync(
        WebhookSubscriptionRequest request,
        IGrantUrlTesterService grantUrlTester,
        WebhooksContext context,
        ClaimsPrincipal user)
    {
        var grantOk = await grantUrlTester.TestGrantUrl(request.Url, request.GrantUrl, request.Token ?? string.Empty);

        if (grantOk)
        {
            var subscription = new WebhookSubscription()
            {
                Date = DateTime.UtcNow,
                DestUrl = request.Url,
                Token = request.Token,
                Type = Enum.Parse<WebhookType>(request.Event, ignoreCase: true),
                UserId = user.GetUserId()!
            };

            context.Add(subscription);
            await context.SaveChangesAsync();

            return TypedResults.Created($"/api/webhooks/{subscription.Id}");
        }
        else
        {
            return TypedResults.BadRequest($"Invalid grant URL: {request.GrantUrl}");
        }
    }

    /// <summary>
    /// Deletes a webhook subscription owned by the current user.
    /// </summary>
    public static async Task<Results<Accepted, NotFound<string>>> DeleteAsync(
        WebhooksContext context,
        ClaimsPrincipal user,
        int id)
    {
        var userId = user.GetUserId();
        var subscription = await context.Subscriptions.SingleOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (subscription != null)
        {
            context.Remove(subscription);
            await context.SaveChangesAsync();
            return TypedResults.Accepted($"/api/webhooks/{subscription.Id}");
        }

        return TypedResults.NotFound($"Subscriptions {id} not found");
    }
}