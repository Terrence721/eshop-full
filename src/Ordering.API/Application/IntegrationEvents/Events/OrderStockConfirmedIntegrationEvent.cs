namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// Integration event signalling stock was confirmed for an order.
/// </summary>
public record OrderStockConfirmedIntegrationEvent(int orderId) : IntegrationEvent
{
    public int OrderId { get; } = orderId;
}
