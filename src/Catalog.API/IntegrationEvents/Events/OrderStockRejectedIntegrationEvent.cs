namespace eShop.Catalog.API.IntegrationEvents.Events
{
    /// <summary>
    /// Integration event indicating that the requested stock is unavailable for at least one item.
    /// </summary>
    public record OrderStockRejectedIntegrationEvent(int OrderId, List<ConfirmedOrderStockItem> OrderStockItems) : IntegrationEvent;
}
