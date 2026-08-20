namespace eShop.IntegrationEventLogEF.Services;

public class IntegrationEventLogService<TContext> : IIntegrationEventLogService, IDisposable
    where TContext : DbContext
{
    private static readonly IReadOnlyDictionary<string, Type> s_eventTypesByShortName = BuildEventTypesByShortName();

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
            .Select(e => e.DeserializeJsonContent(ResolveEventType(e.EventTypeShortName)))
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

    private static Type ResolveEventType(string eventTypeShortName)
    {
        if (!s_eventTypesByShortName.TryGetValue(eventTypeShortName, out var eventType))
        {
            throw new InvalidOperationException(
                $"No integration event type named '{eventTypeShortName}' was found in the entry assembly.");
        }

        return eventType;
    }

    private static IReadOnlyDictionary<string, Type> BuildEventTypesByShortName()
    {
        var entryAssembly = Assembly.GetEntryAssembly()
            ?? throw new InvalidOperationException("No entry assembly is available to resolve integration event types from.");

        var eventTypesByShortName = new Dictionary<string, Type>();
        var duplicateNames = new List<string>();

        foreach (var type in entryAssembly.GetTypes().Where(t => t.Name.EndsWith(nameof(IntegrationEvent), StringComparison.Ordinal)))
        {
            if (!eventTypesByShortName.TryAdd(type.Name, type))
            {
                duplicateNames.Add(type.Name);
            }
        }

        if (duplicateNames.Count != 0)
        {
            throw new InvalidOperationException(
                $"Multiple integration event types share the same short name, which the outbox can't disambiguate: {string.Join(", ", duplicateNames)}.");
        }

        return eventTypesByShortName;
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
