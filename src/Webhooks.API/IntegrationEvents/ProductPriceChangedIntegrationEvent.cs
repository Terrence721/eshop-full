namespace Webhooks.API.IntegrationEvents;

/// <summary>
/// Integration event raised when a product's price changes.
/// </summary>
public record ProductPriceChangedIntegrationEvent(int ProductId, decimal NewPrice, decimal OldPrice) : IntegrationEvent;