namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// Integration event publishing stock-confirmed status for an order.
/// </summary>
public record OrderStatusChangedToStockConfirmedIntegrationEvent(
    int OrderId,
    OrderStatus OrderStatus,
    string BuyerName,
    string BuyerIdentityGuid) : IntegrationEvent;
