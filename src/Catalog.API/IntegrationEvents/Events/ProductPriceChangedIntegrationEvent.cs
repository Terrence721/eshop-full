namespace eShop.Catalog.API.IntegrationEvents.Events;

public record ProductPriceChangedIntegrationEvent(int ProductId, decimal NewPrice, decimal OldPrice) : IntegrationEvent;
