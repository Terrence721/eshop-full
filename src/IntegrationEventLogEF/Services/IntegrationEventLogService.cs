namespace eShop.IntegrationEventLogEF.Services;

public class IntegrationEventLogService<TContext> : IIntegrationEventLogService, IDisposable
    where TContext : DbContext
{
    private volatile bool _disposedValue;
    private readonly TContext _context;

    public IntegrationEventLogService(TContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<IntegrationEventLogEntry>> RetrievePendingEventLogsAsync(Guid transactionId)
    {
        var result = await _context.Set<IntegrationEventLogEntry>()
            .Where(e => e.TransactionId == transactionId && e.State == EventState.NotPublished)
            .ToListAsync();

        if (result.Count == 0)
        {
            return [];
        }

        return result
            .OrderBy(e => e.CreationTime)
            .Select(e => e.DeserializeJsonContent(IntegrationEventTypeResolver.Resolve(e.EventTypeShortName)))
            .ToList();
    }

    public Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        var eventLogEntry = new IntegrationEventLogEntry(@event, transaction.TransactionId);

        _context.Database.UseTransaction(transaction.GetDbTransaction());
        _context.Set<IntegrationEventLogEntry>().Add(eventLogEntry);

        return _context.SaveChangesAsync();
    }

    public Task MarkEventAsPublishedAsync(Guid eventId) => UpdateEventStatusAsync(eventId, EventState.Published);

    public Task MarkEventAsInProgressAsync(Guid eventId) => UpdateEventStatusAsync(eventId, EventState.InProgress);

    public Task MarkEventAsFailedAsync(Guid eventId) => UpdateEventStatusAsync(eventId, EventState.PublishedFailed);

    private async Task UpdateEventStatusAsync(Guid eventId, EventState status)
    {
        var eventLogEntry = await _context.Set<IntegrationEventLogEntry>()
            .SingleAsync(e => e.EventId == eventId);

        eventLogEntry.State = status;

        if (status == EventState.InProgress)
        {
            eventLogEntry.TimesSent++;
        }

        await _context.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _context.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
