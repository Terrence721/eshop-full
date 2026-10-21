namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// Integration event confirming the grace period for an order has elapsed.
/// </summary>
public record GracePeriodConfirmedIntegrationEvent : IntegrationEvent
{
    public int OrderId { get; }

    public GracePeriodConfirmedIntegrationEvent(int orderId) =>
        OrderId = orderId;
}
