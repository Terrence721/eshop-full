namespace eShop.Catalog.API.IntegrationEvents.Events;

/// <summary>
/// Represents a single line (product + units) within an order-stock integration event.
/// </summary>
public record OrderStockItem(int ProductId, int Units);
