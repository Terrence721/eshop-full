using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;

namespace eShop.Ordering.FunctionalTests
{
    public sealed class NullEventBus : IEventBus
    {
        public Task PublishAsync(IntegrationEvent @event) => Task.CompletedTask;
    }
}
