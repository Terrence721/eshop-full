using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.EventBusRabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EventBusRabbitMQ.UnitTests;

[TestClass]
public class RabbitMQEventBusTests
{
    private sealed record TestEvent : IntegrationEvent;

    private static RabbitMQEventBus CreateEventBus() => new(
        NullLogger<RabbitMQEventBus>.Instance,
        new ServiceCollection().BuildServiceProvider(),
        Options.Create(new EventBusOptions { SubscriptionClientName = "test" }),
        Options.Create(new EventBusSubscriptionInfo()),
        new RabbitMQTelemetry());

    [TestMethod]
    public async Task PublishAsync_throws_InvalidOperationException_when_connection_is_not_open()
    {
        // Regression test: the connection field is only ever assigned by StartAsync, which this
        // test deliberately never calls, so PublishAsync must fail loudly with the intended message
        // instead of the opaque NullReferenceException it used to throw before this bug was fixed
        // (see todo.md's EventBusRabbitMQ section).
        IEventBus eventBus = CreateEventBus();

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => eventBus.PublishAsync(new TestEvent()));

        Assert.AreEqual("RabbitMQ connection is not open", exception.Message);
    }

    [TestMethod]
    public void Dispose_does_not_throw_when_consumer_channel_was_never_assigned()
    {
        using var eventBus = CreateEventBus();
    }
}
