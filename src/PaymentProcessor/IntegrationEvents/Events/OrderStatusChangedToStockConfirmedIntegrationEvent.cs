namespace eShop.PaymentProcessor.IntegrationEvents.Events;

/// <summary>
/// Integration event raised when an order's status changes to stock confirmed.
/// </summary>
/// <param name="OrderId">The identifier of the order whose stock was confirmed.</param>
public record OrderStatusChangedToStockConfirmedIntegrationEvent(int OrderId) : IntegrationEvent;
