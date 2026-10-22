namespace eShop.PaymentProcessor.IntegrationEvents.Events
{
    /// <summary>
    /// Integration event raised when payment for an order succeeds.
    /// </summary>
    /// <param name="OrderId">The identifier of the order whose payment succeeded.</param>
    public record OrderPaymentSucceededIntegrationEvent(int OrderId) : IntegrationEvent;
}
