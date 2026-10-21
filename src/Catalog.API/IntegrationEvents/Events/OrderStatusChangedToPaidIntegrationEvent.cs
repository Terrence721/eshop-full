namespace eShop.Catalog.API.IntegrationEvents.Events;

/// <summary>
/// Integration event requesting stock decrement after an order has been paid.
/// </summary>
public record OrderStatusChangedToPaidIntegrationEvent(int OrderId, IEnumerable<OrderStockItem> OrderStockItems) : IntegrationEvent;
