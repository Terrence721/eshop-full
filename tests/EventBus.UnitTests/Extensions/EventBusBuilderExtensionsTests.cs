using System.Text.Json;
using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace eShop.EventBus.UnitTests.Extensions
{
    [TestClass]
    public class EventBusBuilderExtensionsTests
    {
        private sealed record TestEvent : IntegrationEvent;

        private sealed class TestHandler : IIntegrationEventHandler<TestEvent>
        {
            public Task Handle(TestEvent @event) => Task.CompletedTask;
        }

        private sealed record SecondTestEvent : IntegrationEvent;

        private sealed class SecondTestHandler : IIntegrationEventHandler<SecondTestEvent>
        {
            public Task Handle(SecondTestEvent @event) => Task.CompletedTask;
        }

        private sealed class DuplicateTestHandler : IIntegrationEventHandler<TestEvent>
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
        public void ConfigureJsonOptions_returns_the_same_builder_for_fluent_chaining()
        {
            var builder = new TestEventBusBuilder();
            var returned = builder.ConfigureJsonOptions(o => o.WriteIndented = true);

            Assert.AreSame(builder, returned);
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
        public void AddSubscription_returns_the_same_builder_for_fluent_chaining()
        {
            var builder = new TestEventBusBuilder();
            var returned = builder.AddSubscription<TestEvent, TestHandler>();

            Assert.AreSame(builder, returned);
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

        [TestMethod]
        public void Multiple_AddSubscription_calls_accumulate_all_event_types()
        {
            var builder = new TestEventBusBuilder();

            builder.AddSubscription<TestEvent, TestHandler>()
                   .AddSubscription<SecondTestEvent, SecondTestHandler>();

            using var provider = builder.Services.BuildServiceProvider();
            var info = provider.GetRequiredService<IOptions<EventBusSubscriptionInfo>>().Value;

            Assert.AreEqual(2, info.EventTypes.Count);
            Assert.AreEqual(typeof(TestEvent), info.EventTypes[nameof(TestEvent)]);
            Assert.AreEqual(typeof(SecondTestEvent), info.EventTypes[nameof(SecondTestEvent)]);
        }

        [TestMethod]
        public void Multiple_AddSubscription_calls_register_separate_keyed_services()
        {
            var builder = new TestEventBusBuilder();

            builder.AddSubscription<TestEvent, TestHandler>()
                   .AddSubscription<SecondTestEvent, SecondTestHandler>();

            using var provider = builder.Services.BuildServiceProvider();
            var testHandler = provider.GetKeyedService<IIntegrationEventHandler>(typeof(TestEvent));
            var secondHandler = provider.GetKeyedService<IIntegrationEventHandler>(typeof(SecondTestEvent));

            Assert.IsInstanceOfType<TestHandler>(testHandler);
            Assert.IsInstanceOfType<SecondTestHandler>(secondHandler);
        }

        [TestMethod]
        public void Two_subscriptions_with_the_same_event_type_register_two_handlers()
        {
            var builder = new TestEventBusBuilder();

            builder.AddSubscription<TestEvent, TestHandler>()
                   .AddSubscription<TestEvent, DuplicateTestHandler>();

            using var provider = builder.Services.BuildServiceProvider();
            var handlers = provider.GetKeyedServices<IIntegrationEventHandler>(typeof(TestEvent)).ToList();

            Assert.AreEqual(2, handlers.Count);
        }
    }
}