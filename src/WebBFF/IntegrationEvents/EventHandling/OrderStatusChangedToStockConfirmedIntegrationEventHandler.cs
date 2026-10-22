namespace eShop.WebBFF.IntegrationEvents.EventHandling
{
    public class OrderStatusChangedToStockConfirmedIntegrationEventHandler(
        OrderStatusNotifications notifications,
        ILogger<OrderStatusChangedToStockConfirmedIntegrationEventHandler> logger) :
        IIntegrationEventHandler<OrderStatusChangedToStockConfirmedIntegrationEvent>
    {
        public Task Handle(OrderStatusChangedToStockConfirmedIntegrationEvent @event)
        {
            logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

            return notifications.NotifyOrderStatusChangedAsync(@event.BuyerIdentityGuid);
        }
    }
}
