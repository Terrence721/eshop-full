namespace eShop.WebhookClient.Services;

/// <summary>
/// HTTP client for the Webhooks.API subscription endpoints.
/// </summary>
public class WebhooksClient(HttpClient client)
{
    /// <summary>
    /// Registers a new webhook subscription.
    /// </summary>
    public Task<HttpResponseMessage> AddWebHookAsync(WebhookSubscriptionRequest payload)
    {
        return client.PostAsJsonAsync("/api/webhooks", payload);
    }

    /// <summary>
    /// Loads all registered webhook subscriptions.
    /// </summary>
    public async Task<IEnumerable<WebhookResponse>> LoadWebhooks()
    {
        return await client.GetFromJsonAsync<IEnumerable<WebhookResponse>>("/api/webhooks") ?? [];
    }
}