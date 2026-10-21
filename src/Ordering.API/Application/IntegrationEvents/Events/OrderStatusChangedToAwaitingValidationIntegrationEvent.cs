namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// Integration event publishing awaiting-validation status and stock information.
/// </summary>
public record OrderStatusChangedToAwaitingValidationIntegrationEvent(
    int OrderId,
    OrderStatus OrderStatus,
    string BuyerName,
    string BuyerIdentityGuid,
    IEnumerable<OrderStockItem> OrderStockItems) : IntegrationEvent;

public record OrderStockItem(int ProductId, int Units);
