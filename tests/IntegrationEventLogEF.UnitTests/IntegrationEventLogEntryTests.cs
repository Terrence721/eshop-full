using System.Text.Json;
using eShop.EventBus.Events;
using eShop.IntegrationEventLogEF;

namespace IntegrationEventLogEF.UnitTests;

[TestClass]
public class IntegrationEventLogEntryTests
{
    private sealed record TestEvent : IntegrationEvent
    {
        public string Payload { get; init; } = string.Empty;
    }

    private sealed record UnrelatedPayload
    {
        public string Value { get; init; } = string.Empty;
    }

    [TestMethod]
    public void Constructor_maps_event_and_transaction_fields()
    {
        var @event = new TestEvent { Payload = "order-placed" };
        var transactionId = Guid.NewGuid();

        var entry = new IntegrationEventLogEntry(@event, transactionId);

        Assert.AreEqual(@event.Id, entry.EventId);
        Assert.AreEqual(@event.CreationDate, entry.CreationTime);
        Assert.AreEqual(typeof(TestEvent).FullName, entry.EventTypeName);
        Assert.AreEqual(EventState.NotPublished, entry.State);
        Assert.AreEqual(0, entry.TimesSent);
        Assert.AreEqual(transactionId, entry.TransactionId);
    }

    [TestMethod]
    public void Constructor_serializes_the_event_as_indented_JSON()
    {
        var entry = new IntegrationEventLogEntry(new TestEvent { Payload = "order-placed" }, Guid.NewGuid());

        Assert.IsTrue(entry.Content.Contains('\n'), "Expected WriteIndented output to contain newlines.");
        StringAssert.Contains(entry.Content, "order-placed");
    }

    [TestMethod]
    public void EventTypeShortName_returns_the_segment_after_the_last_dot()
    {
        var entry = new IntegrationEventLogEntry(new TestEvent(), Guid.NewGuid());

        var expectedShortName = typeof(TestEvent).FullName!.Split('.').Last();
        Assert.AreEqual(expectedShortName, entry.EventTypeShortName);
    }

    [TestMethod]
    public void DeserializeJsonContent_round_trips_the_original_event_and_returns_the_same_instance()
    {
        var original = new TestEvent { Payload = "order-placed" };
        var entry = new IntegrationEventLogEntry(original, Guid.NewGuid());

        var result = entry.DeserializeJsonContent(typeof(TestEvent));

        Assert.AreSame(entry, result);
        Assert.IsInstanceOfType<TestEvent>(entry.IntegrationEvent);
        var deserialized = (TestEvent)entry.IntegrationEvent!;
        Assert.AreEqual(original.Payload, deserialized.Payload);
        Assert.AreEqual(original.Id, deserialized.Id);
    }

    [TestMethod]
    public void DeserializeJsonContent_sets_IntegrationEvent_to_null_when_the_content_is_not_an_IntegrationEvent()
    {
        var entry = new IntegrationEventLogEntry(new TestEvent(), Guid.NewGuid())
        {
            Content = JsonSerializer.Serialize(new UnrelatedPayload { Value = "x" })
        };

        entry.DeserializeJsonContent(typeof(UnrelatedPayload));

        Assert.IsNull(entry.IntegrationEvent);
    }
}
