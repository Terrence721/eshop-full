namespace eShop.EventBus.Events;

public record IntegrationEvent
{
    public IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }

    public Guid Id { get; init; }

    public DateTime CreationDate { get; init; }
}
