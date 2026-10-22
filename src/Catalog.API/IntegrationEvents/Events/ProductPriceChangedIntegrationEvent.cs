namespace eShop.Catalog.API.IntegrationEvents.Events
{
    /// <summary>
    /// Integration event raised when a catalog item's price has changed.
    /// </summary>
    public record ProductPriceChangedIntegrationEvent(int ProductId, decimal NewPrice, decimal OldPrice) : IntegrationEvent;
}
