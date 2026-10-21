namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// Integration event publishing shipped status for an order.
/// </summary>
public record OrderStatusChangedToShippedIntegrationEvent(
    int OrderId,
    OrderStatus OrderStatus,
    string BuyerName,
    string BuyerIdentityGuid) : IntegrationEvent;
