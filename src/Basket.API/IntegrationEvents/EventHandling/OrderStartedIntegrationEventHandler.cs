using eShop.Basket.API.IntegrationEvents.Events;
using eShop.Basket.API.Repositories;

namespace eShop.Basket.API.IntegrationEvents.EventHandling
{
    /// <summary>
    /// Handles <see cref="OrderStartedIntegrationEvent" /> by removing the corresponding customer's basket.
    /// </summary>
    public class OrderStartedIntegrationEventHandler(
        IBasketRepository repository,
        ILogger<OrderStartedIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderStartedIntegrationEvent>
    {
        public async Task Handle(OrderStartedIntegrationEvent @event)
        {
            logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

            await repository.DeleteBasketAsync(@event.UserId);
        }
    }
}
