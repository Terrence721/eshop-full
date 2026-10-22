namespace eShop.Ordering.API.Application.IntegrationEvents.Events
{
    /// <summary>
    /// Integration event signalling payment succeeded for an order.
    /// </summary>
    public record OrderPaymentSucceededIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; }

        public OrderPaymentSucceededIntegrationEvent(int orderId) => OrderId = orderId;
    }
}
