using System.Diagnostics;
using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.EventBusRabbitMQ;

namespace EventBusRabbitMQ.UnitTests;

[TestClass]
public class TelemetryEventBusDecoratorTests
{
    private sealed record TestEvent : IntegrationEvent;

    private sealed class FakeEventBus : IEventBus
    {
        public int CallCount { get; private set; }
        public IntegrationEvent? Received { get; private set; }
        public Exception? ExceptionToThrow { get; set; }

        public Task PublishAsync(IntegrationEvent @event)
        {
            CallCount++;
            Received = @event;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.CompletedTask;
        }
    }

    private static IDisposable ListenToAllActivities()
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    [TestMethod]
    public async Task PublishAsync_delegates_to_the_inner_event_bus()
    {
        using var _ = ListenToAllActivities();
        var inner = new FakeEventBus();
        var decorator = new TelemetryEventBusDecorator(inner, new RabbitMQTelemetry());
        var @event = new TestEvent();

        await decorator.PublishAsync(@event);

        Assert.AreEqual(1, inner.CallCount);
        Assert.AreSame(@event, inner.Received);
    }

    [TestMethod]
    public async Task PublishAsync_tags_the_exception_and_rethrows_when_inner_throws()
    {
        using var _ = ListenToAllActivities();
        var thrown = new InvalidOperationException("publish failed");
        var inner = new FakeEventBus { ExceptionToThrow = thrown };
        var decorator = new TelemetryEventBusDecorator(inner, new RabbitMQTelemetry());

        var caught = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => decorator.PublishAsync(new TestEvent()));

        Assert.AreSame(thrown, caught);
    }

    [TestMethod]
    public async Task PublishAsync_starts_an_activity_named_for_the_publish_operation()
    {
        var started = new List<Activity>();
        using var nameListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == RabbitMQTelemetry.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = started.Add,
        };
        ActivitySource.AddActivityListener(nameListener);
        var decorator = new TelemetryEventBusDecorator(new FakeEventBus(), new RabbitMQTelemetry());

        await decorator.PublishAsync(new TestEvent());

        Assert.AreEqual(1, started.Count);
        Assert.AreEqual($"{nameof(TestEvent)} publish", started[0].DisplayName);
        Assert.AreEqual("rabbitmq", started[0].GetTagItem("messaging.system"));
    }
}
