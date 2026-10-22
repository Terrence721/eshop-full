namespace Webhooks.API.IntegrationEvents;

/// <summary>
/// Handles <see cref="OrderStatusChangedToPaidIntegrationEvent"/> by notifying paid webhook subscribers.
/// </summary>
public class OrderStatusChangedToPaidIntegrationEventHandler(
    IWebhooksRetriever retriever,
    IWebhooksSender sender,
    ILogger<OrderStatusChangedToPaidIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderStatusChangedToPaidIntegrationEvent>
{
    /// <summary>
    /// Retrieves paid-order webhook subscriptions and dispatches the event payload to each.
    /// </summary>
    public async Task Handle(OrderStatusChangedToPaidIntegrationEvent @event)
    {
        var subscriptions = await retriever.GetSubscriptionsOfType(WebhookType.OrderPaid);

        logger.LogInformation("Received OrderStatusChangedToPaidIntegrationEvent and got {SubscriptionsCount} subscriptions to process", subscriptions.Count());

        var whook = new WebhookData(WebhookType.OrderPaid, @event);

        await sender.SendAll(subscriptions, whook);
    }
}