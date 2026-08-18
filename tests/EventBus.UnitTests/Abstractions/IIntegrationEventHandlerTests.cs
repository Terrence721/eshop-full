using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;

namespace EventBus.UnitTests.Abstractions;

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

    [TestMethod]
    public async Task Handle_on_the_non_generic_interface_dispatches_to_the_typed_Handle_overload()
    {
        var handler = new TestIntegrationEventHandler();
        var testEvent = new TestIntegrationEvent();

        await ((IIntegrationEventHandler)handler).Handle(testEvent);

        Assert.AreSame(testEvent, handler.Received);
    }
}
