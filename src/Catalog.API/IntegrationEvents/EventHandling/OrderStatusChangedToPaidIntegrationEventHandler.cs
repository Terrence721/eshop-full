namespace eShop.Catalog.API.IntegrationEvents.EventHandling
{
    /// <summary>
    /// Handles paid order events by decrementing stock for each ordered item.
    /// </summary>
    public class OrderStatusChangedToPaidIntegrationEventHandler(
        CatalogContext catalogContext,
        ILogger<OrderStatusChangedToPaidIntegrationEventHandler> logger) :
        IIntegrationEventHandler<OrderStatusChangedToPaidIntegrationEvent>
    {
        public async Task Handle(OrderStatusChangedToPaidIntegrationEvent @event)
        {
            logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

            //we're not blocking stock/inventory
            foreach (var orderStockItem in @event.OrderStockItems)
            {
                var catalogItem = catalogContext.CatalogItems.Find(orderStockItem.ProductId);

                catalogItem?.RemoveStock(orderStockItem.Units);
            }

            await catalogContext.SaveChangesAsync();
        }
    }
}
