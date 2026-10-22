namespace eShop.WebBFF.IntegrationEvents.EventHandling
{
    public class OrderStatusChangedToSubmittedIntegrationEventHandler(
        OrderStatusNotifications notifications,
        ILogger<OrderStatusChangedToSubmittedIntegrationEventHandler> logger) :
        IIntegrationEventHandler<OrderStatusChangedToSubmittedIntegrationEvent>
    {
        public Task Handle(OrderStatusChangedToSubmittedIntegrationEvent @event)
        {
            logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

            return notifications.NotifyOrderStatusChangedAsync(@event.BuyerIdentityGuid);
        }
    }
}
