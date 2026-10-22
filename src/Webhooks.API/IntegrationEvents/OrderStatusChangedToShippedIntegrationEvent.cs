namespace Webhooks.API.IntegrationEvents
{
    /// <summary>
    /// Integration event raised when an order's status changes to shipped.
    /// </summary>
    public record OrderStatusChangedToShippedIntegrationEvent(int OrderId, string OrderStatus, string BuyerName) : IntegrationEvent;
}