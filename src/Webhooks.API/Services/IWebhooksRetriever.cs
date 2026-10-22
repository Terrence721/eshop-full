namespace Webhooks.API.Services;

/// <summary>
/// Retrieves webhook subscriptions filtered by type.
/// </summary>
public interface IWebhooksRetriever
{
    /// <summary>
    /// Returns all subscriptions of the given <paramref name="type" />.
    /// </summary>
    Task<IEnumerable<WebhookSubscription>> GetSubscriptionsOfType(WebhookType type);
}