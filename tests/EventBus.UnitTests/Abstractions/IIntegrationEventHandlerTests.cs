using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;

namespace eShop.EventBus.UnitTests.Abstractions
{
    [TestClass]
    public class IIntegrationEventHandlerTests
    {
        private sealed record TestIntegrationEvent : IntegrationEvent;

        private sealed class TestIntegrationEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
        {
            public TestIntegrationEvent? Received { get; private set; }

            public Task Handle(TestIntegrationEvent @event)
            {
                Received = @event;
                return Task.CompletedTask;
            }
        }

        private sealed class ThrowingHandler : IIntegrationEventHandler<TestIntegrationEvent>
        {
            public Task Handle(TestIntegrationEvent @event) => throw new InvalidOperationException("expected failure");
        }

        [TestMethod]
        public async Task Handle_on_the_non_generic_interface_dispatches_to_the_typed_Handle_overload()
        {
            var handler = new TestIntegrationEventHandler();
            var testEvent = new TestIntegrationEvent();

            await ((IIntegrationEventHandler)handler).Handle(testEvent);

            Assert.AreSame(testEvent, handler.Received);
        }

        [TestMethod]
        public void Non_generic_interface_Handle_allows_casting_from_base_type()
        {
            var handler = new TestIntegrationEventHandler();
            IntegrationEvent baseEvent = new TestIntegrationEvent();

            Assert.IsTrue(baseEvent is TestIntegrationEvent);
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task Handle_propagates_exceptions_from_typed_handler()
        {
            var handler = new ThrowingHandler();
            var testEvent = new TestIntegrationEvent();

            Exception? caughtEx = null;
            try
            {
                await ((IIntegrationEventHandler)handler).Handle(testEvent);
            }
            catch (InvalidOperationException ex)
            {
                caughtEx = ex;
            }

            Assert.IsNotNull(caughtEx);
            Assert.AreEqual("expected failure", caughtEx!.Message);
        }
    }
}
