namespace eShop.WebBFF.IntegrationEvents.EventHandling;

public class OrderStatusChangedToCancelledIntegrationEventHandler(
    OrderStatusNotifications notifications,
    ILogger<OrderStatusChangedToCancelledIntegrationEventHandler> logger) :
    IIntegrationEventHandler<OrderStatusChangedToCancelledIntegrationEvent>
{
    public Task Handle(OrderStatusChangedToCancelledIntegrationEvent @event)
    {
        logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

        return notifications.NotifyOrderStatusChangedAsync(@event.BuyerIdentityGuid);
    }
}
