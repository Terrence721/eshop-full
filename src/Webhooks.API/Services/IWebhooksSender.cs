namespace Webhooks.API.Services;

/// <summary>
/// Sends a webhook payload to a set of receivers.
/// </summary>
public interface IWebhooksSender
{
    /// <summary>
    /// Sends <paramref name="data" /> to every receiver in <paramref name="receivers" />.
    /// </summary>
    Task SendAll(IEnumerable<WebhookSubscription> receivers, WebhookData data);
}