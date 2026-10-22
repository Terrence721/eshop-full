namespace Webhooks.API.IntegrationEvents;

/// <summary>
/// Handles <see cref="OrderStatusChangedToShippedIntegrationEvent"/> by notifying shipped webhook subscribers.
/// </summary>
public class OrderStatusChangedToShippedIntegrationEventHandler(
    IWebhooksRetriever retriever,
    IWebhooksSender sender,
    ILogger<OrderStatusChangedToShippedIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderStatusChangedToShippedIntegrationEvent>
{
    /// <summary>
    /// Retrieves shipped-order webhook subscriptions and dispatches the event payload to each.
    /// </summary>
    public async Task Handle(OrderStatusChangedToShippedIntegrationEvent @event)
    {
        var subscriptions = await retriever.GetSubscriptionsOfType(WebhookType.OrderShipped);

        logger.LogInformation("Received OrderStatusChangedToShippedIntegrationEvent and got {SubscriptionCount} subscriptions to process", subscriptions.Count());

        var whook = new WebhookData(WebhookType.OrderShipped, @event);

        await sender.SendAll(subscriptions, whook);
    }
}