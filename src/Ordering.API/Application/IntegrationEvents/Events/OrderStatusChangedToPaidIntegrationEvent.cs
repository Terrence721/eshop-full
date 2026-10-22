namespace eShop.Ordering.API.Application.IntegrationEvents.Events
{
    /// <summary>
    /// Integration event publishing paid status and replenished stock information.
    /// </summary>
    public record OrderStatusChangedToPaidIntegrationEvent(
        int OrderId,
        OrderStatus OrderStatus,
        string BuyerName,
        string BuyerIdentityGuid,
        IEnumerable<OrderStockItem> OrderStockItems) : IntegrationEvent;
}
