using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EventBus.UnitTests.Extensions;

[TestClass]
public class EventBusBuilderExtensionsTests
{
    private sealed record TestEvent : IntegrationEvent;

    private sealed class TestHandler : IIntegrationEventHandler<TestEvent>
    {
        public Task Handle(TestEvent @event) => Task.CompletedTask;
    }

    private sealed class TestEventBusBuilder : IEventBusBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
    }

    [TestMethod]
    public void ConfigureJsonOptions_applies_the_configure_action_to_the_subscription_JsonSerializerOptions()
    {
        var builder = new TestEventBusBuilder();

        builder.ConfigureJsonOptions(o => o.WriteIndented = true);

        using var provider = builder.Services.BuildServiceProvider();
        var info = provider.GetRequiredService<IOptions<EventBusSubscriptionInfo>>().Value;

        Assert.IsTrue(info.JsonSerializerOptions.WriteIndented);
    }

    [TestMethod]
    public void AddSubscription_registers_the_handler_as_a_keyed_transient_service()
    {
        var builder = new TestEventBusBuilder();

        builder.AddSubscription<TestEvent, TestHandler>();

        using var provider = builder.Services.BuildServiceProvider();
        var handler = provider.GetRequiredKeyedService<IIntegrationEventHandler>(typeof(TestEvent));

        Assert.IsInstanceOfType<TestHandler>(handler);
    }

    [TestMethod]
    public void AddSubscription_records_the_event_type_in_EventTypes()
    {
        var builder = new TestEventBusBuilder();

        builder.AddSubscription<TestEvent, TestHandler>();

        using var provider = builder.Services.BuildServiceProvider();
        var info = provider.GetRequiredService<IOptions<EventBusSubscriptionInfo>>().Value;

        Assert.AreEqual(typeof(TestEvent), info.EventTypes[nameof(TestEvent)]);
    }
}
