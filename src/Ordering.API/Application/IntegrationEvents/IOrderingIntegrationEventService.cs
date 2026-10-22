namespace eShop.Ordering.API.Application.IntegrationEvents;

/// <summary>
/// Abstraction for enqueuing integration events to the outbox and publishing them.
/// </summary>
public interface IOrderingIntegrationEventService
{
    Task PublishEventsThroughEventBusAsync(Guid transactionId);
    Task AddAndSaveEventAsync(IntegrationEvent evt);
}
