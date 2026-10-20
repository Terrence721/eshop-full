namespace eShop.Ordering.API.Application.IntegrationEvents;

public class OrderingIntegrationEventService(IEventBus eventBus,
    OrderingContext orderingContext,
    IIntegrationEventLogService integrationEventLogService,
    ILogger<OrderingIntegrationEventService> logger) : IOrderingIntegrationEventService
{
    private readonly IEventBus _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    private readonly OrderingContext _orderingContext = orderingContext ?? throw new ArgumentNullException(nameof(orderingContext));
    private readonly IIntegrationEventLogService _eventLogService = integrationEventLogService ?? throw new ArgumentNullException(nameof(integrationEventLogService));
    private readonly ILogger<OrderingIntegrationEventService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task PublishEventsThroughEventBusAsync(Guid transactionId)
    {
        var pendingLogEvents = await _eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId);

        foreach (var logEvt in pendingLogEvents)
        {
            _logger.LogInformation("Publishing integration event: {IntegrationEventId} - ({@IntegrationEvent})", logEvt.EventId, logEvt.IntegrationEvent);

            try
            {
                await _eventLogService.MarkEventAsInProgressAsync(logEvt.EventId);
                // Every entry yielded by RetrieveEventLogsPendingToPublishAsync already went
                // through DeserializeJsonContent() in its query projection, so IntegrationEvent
                // is always populated here despite the property's nullable declared type.
                await _eventBus.PublishAsync(logEvt.IntegrationEvent!);
                await _eventLogService.MarkEventAsPublishedAsync(logEvt.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing integration event: {IntegrationEventId}", logEvt.EventId);

                await _eventLogService.MarkEventAsFailedAsync(logEvt.EventId);
            }
        }
    }

    public async Task AddAndSaveEventAsync(IntegrationEvent evt)
    {
        _logger.LogInformation("Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})", evt.Id, evt);

        // TransactionBehavior<,> guarantees a transaction is already active by the time any
        // command handler (and therefore this method) runs -- it begins one itself before
        // calling next() if one wasn't already active. Never genuinely null here.
        await _eventLogService.SaveEventAsync(evt, _orderingContext.GetCurrentTransaction()!);
    }
}
