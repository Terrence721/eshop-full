namespace Webhooks.API.IntegrationEvents
{
    /// <summary>
    /// Handles <see cref="ProductPriceChangedIntegrationEvent"/> by notifying price-change webhook subscribers.
    /// </summary>
    public class ProductPriceChangedIntegrationEventHandler(
        IWebhooksRetriever retriever,
        IWebhooksSender sender,
        ILogger<ProductPriceChangedIntegrationEventHandler> logger) : IIntegrationEventHandler<ProductPriceChangedIntegrationEvent>
    {
        /// <summary>
        /// Retrieves price-change webhook subscriptions and dispatches the event payload to each.
        /// </summary>
        public async Task Handle(ProductPriceChangedIntegrationEvent @event)
        {
            var subscriptions = await retriever.GetSubscriptionsOfType(WebhookType.CatalogItemPriceChange);

            logger.LogInformation("Received ProductPriceChangedIntegrationEvent and got {SubscriptionsCount} subscriptions to process", subscriptions.Count());

            var whook = new WebhookData(WebhookType.CatalogItemPriceChange, @event);

            await sender.SendAll(subscriptions, whook);
        }
    }
}