namespace eShop.Catalog.API.IntegrationEvents
{
    /// <summary>
    /// Contract for the Catalog integration event service used by handlers/endpoints.
    /// </summary>
    public interface ICatalogIntegrationEventService
    {
        Task SaveEventAndCatalogContextChangesAsync(IntegrationEvent evt);
        Task PublishThroughEventBusAsync(IntegrationEvent evt);
    }
}
