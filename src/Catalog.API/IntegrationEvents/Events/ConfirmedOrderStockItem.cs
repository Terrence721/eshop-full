namespace eShop.Catalog.API.IntegrationEvents.Events;

/// <summary>
/// Reports whether a single ordered stock item could be satisfied during validation.
/// </summary>
public record ConfirmedOrderStockItem(int ProductId, bool HasStock);
