namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// Integration event publishing submitted status for an order.
/// </summary>
public record OrderStatusChangedToSubmittedIntegrationEvent(
    int OrderId,
    OrderStatus OrderStatus,
    string BuyerName,
    string BuyerIdentityGuid) : IntegrationEvent;
