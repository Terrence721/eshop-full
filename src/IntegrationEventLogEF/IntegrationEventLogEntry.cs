using System.Diagnostics.CodeAnalysis;

namespace eShop.IntegrationEventLogEF;

public class IntegrationEventLogEntry
{
    private static readonly JsonSerializerOptions s_indentedOptions = new() { WriteIndented = true };
    private static readonly JsonSerializerOptions s_caseInsensitiveOptions = new() { PropertyNameCaseInsensitive = true };

    private IntegrationEventLogEntry() { }

    [SetsRequiredMembers]
    public IntegrationEventLogEntry(IntegrationEvent @event, Guid transactionId)
    {
        EventId = @event.Id;
        CreationTime = @event.CreationDate;
        EventTypeName = @event.GetType().FullName
            ?? throw new InvalidOperationException($"Integration event type '{@event.GetType()}' has no FullName.");
        Content = JsonSerializer.Serialize(@event, @event.GetType(), s_indentedOptions);
        State = EventState.NotPublished;
        TimesSent = 0;
        TransactionId = transactionId;
    }
    public Guid EventId { get; init; }
    public required string EventTypeName { get; init; }
    [NotMapped]
    public string EventTypeShortName => EventTypeName.Split('.').Last();
    [NotMapped]
    public IntegrationEvent? IntegrationEvent { get; private set; }
    public EventState State { get; set; }
    public int TimesSent { get; set; }
    public DateTime CreationTime { get; init; }
    public required string Content { get; init; }
    public Guid TransactionId { get; init; }

    public IntegrationEventLogEntry DeserializeJsonContent(Type type)
    {
        IntegrationEvent = JsonSerializer.Deserialize(Content, type, s_caseInsensitiveOptions) as IntegrationEvent;
        return this;
    }
}
