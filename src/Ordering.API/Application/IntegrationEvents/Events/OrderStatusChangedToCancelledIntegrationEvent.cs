namespace eShop.Ordering.API.Application.IntegrationEvents.Events
{
    /// <summary>
    /// Integration event publishing cancellation status for an order.
    /// </summary>
    public record OrderStatusChangedToCancelledIntegrationEvent(
        int OrderId,
        OrderStatus OrderStatus,
        string BuyerName,
        string BuyerIdentityGuid) : IntegrationEvent;
}
