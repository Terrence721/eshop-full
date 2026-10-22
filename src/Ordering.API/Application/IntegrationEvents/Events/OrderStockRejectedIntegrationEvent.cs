namespace eShop.Ordering.API.Application.IntegrationEvents.Events
{
    /// <summary>
    /// Integration event signalling stock was rejected for an order.
    /// </summary>
    public record OrderStockRejectedIntegrationEvent(
        int orderId,
        List<ConfirmedOrderStockItem> orderStockItems) : IntegrationEvent
    {
        public int OrderId { get; } = orderId;

        public List<ConfirmedOrderStockItem> OrderStockItems { get; } = orderStockItems;
    }

    /// <summary>
    /// A rejected stock item and whether stock was available for it.
    /// </summary>
    public record ConfirmedOrderStockItem(int productId, bool hasStock)
    {
        public int ProductId { get; } = productId;

        public bool HasStock { get; } = hasStock;
    }
}
