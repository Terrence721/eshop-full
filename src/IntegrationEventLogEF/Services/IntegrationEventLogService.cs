namespace eShop.IntegrationEventLogEF.Services;

public sealed class IntegrationEventLogService<TContext> : IIntegrationEventLogService, IDisposable
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

    // Composition-over-inheritance fix: the full protected-virtual Dispose(bool)
    // template-method pattern (plus GC.SuppressFinalize) exists to let a
    // subclass release its own unmanaged resources or a finalizer clean up
    // safely -- this class has neither (no derived types, no finalizer), so
    // the virtual extension point served no purpose. Dispose() still guards
    // against a second call: IDisposable.Dispose() is conventionally
    // idempotent but that's not a guaranteed contract, so this keeps the
    // real safety property while dropping the unused override hook.
    public void Dispose()
    {
        if (_disposedValue)
        {
            return;
        }

        _context.Dispose();
        _disposedValue = true;
    }
}
