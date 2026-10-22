namespace eShop.EventBusKafka
{
    // Same seam EventBusRabbitMQ already has, duplicated here rather than shared across the two
    // projects (EventBusRabbitMQ is being removed once every consumer migrates, so a dependency on
    // it would be backwards) -- lets the wire format be swapped without touching transport code.
    public interface IEventSerializer
    {
        byte[] Serialize(IntegrationEvent @event);

        IntegrationEvent? Deserialize(string message, Type eventType);
    }
}
