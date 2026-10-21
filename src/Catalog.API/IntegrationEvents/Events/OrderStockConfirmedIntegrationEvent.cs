namespace eShop.Catalog.API.IntegrationEvents.Events;

/// <summary>
/// Integration event indicating the requested stock is available for an order.
/// </summary>
public record OrderStockConfirmedIntegrationEvent(int OrderId) : IntegrationEvent;
