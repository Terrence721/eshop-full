namespace eShop.WebBFF.IntegrationEvents.EventHandling;

public class OrderStatusChangedToShippedIntegrationEventHandler(
    OrderStatusNotifications notifications,
    ILogger<OrderStatusChangedToShippedIntegrationEventHandler> logger) :
    IIntegrationEventHandler<OrderStatusChangedToShippedIntegrationEvent>
{
    public Task Handle(OrderStatusChangedToShippedIntegrationEvent @event)
    {
        logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

        return notifications.NotifyOrderStatusChangedAsync(@event.BuyerIdentityGuid);
    }
}
