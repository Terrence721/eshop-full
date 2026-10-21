namespace eShop.Catalog.API.IntegrationEvents.Events;

/// <summary>
/// Integration event requesting stock validation for an order.
/// </summary>
public record OrderStatusChangedToAwaitingValidationIntegrationEvent(int OrderId, IEnumerable<OrderStockItem> OrderStockItems) : IntegrationEvent;
