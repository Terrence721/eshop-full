namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// Integration event signalling payment failed for an order.
/// </summary>
public record OrderPaymentFailedIntegrationEvent : IntegrationEvent
{
    public int OrderId { get; }

    public OrderPaymentFailedIntegrationEvent(int orderId) => OrderId = orderId;
}
