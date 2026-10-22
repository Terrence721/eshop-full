namespace eShop.PaymentProcessor.IntegrationEvents.Events
{
    /// <summary>
    /// Integration event raised when payment for an order fails.
    /// </summary>
    /// <param name="OrderId">The identifier of the order whose payment failed.</param>
    public record OrderPaymentFailedIntegrationEvent(int OrderId) : IntegrationEvent;
}
