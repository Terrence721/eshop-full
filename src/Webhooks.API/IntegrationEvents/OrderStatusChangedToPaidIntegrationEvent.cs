namespace Webhooks.API.IntegrationEvents;

/// <summary>
/// Integration event raised when an order's status changes to paid.
/// </summary>
public record OrderStatusChangedToPaidIntegrationEvent(int OrderId, IEnumerable<OrderStockItem> OrderStockItems) : IntegrationEvent;