namespace Webhooks.API.Services;

/// <summary>
/// Retrieves webhook subscriptions from the Webhooks database.
/// </summary>
public class WebhooksRetriever(WebhooksContext db) : IWebhooksRetriever
{
    /// <summary>
    /// Returns all subscriptions of the given <paramref name="type" />.
    /// </summary>
    public async Task<IEnumerable<WebhookSubscription>> GetSubscriptionsOfType(WebhookType type)
    {
        return await db.Subscriptions.Where(s => s.Type == type).ToListAsync();
    }
}