using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.EventBusRabbitMQ;
using Microsoft.Extensions.Options;

namespace EventBusRabbitMQ.UnitTests;

[TestClass]
public class SystemTextJsonEventSerializerTests
{
    private sealed record TestEvent(string Message) : IntegrationEvent;

    private static SystemTextJsonEventSerializer CreateSerializer() =>
        new(Options.Create(new EventBusSubscriptionInfo()));

    [TestMethod]
    public void Serialize_then_Deserialize_round_trips_the_event()
    {
        var serializer = CreateSerializer();
        var original = new TestEvent("hello");

        var bytes = serializer.Serialize(original);
        var result = serializer.Deserialize(System.Text.Encoding.UTF8.GetString(bytes), typeof(TestEvent));

        var roundTripped = Assert.IsInstanceOfType<TestEvent>(result);
        Assert.AreEqual(original.Id, roundTripped.Id);
        Assert.AreEqual(original.CreationDate, roundTripped.CreationDate);
        Assert.AreEqual(original.Message, roundTripped.Message);
    }

    [TestMethod]
    public void Deserialize_returns_null_for_a_type_that_is_not_an_IntegrationEvent()
    {
        var serializer = CreateSerializer();

        var result = serializer.Deserialize("""{"value":1}""", typeof(NotAnIntegrationEvent));

        Assert.IsNull(result);
    }

    private sealed record NotAnIntegrationEvent(int Value);
}
