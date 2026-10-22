using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.IntegrationEventLogEF;
using eShop.IntegrationEventLogEF.Services;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests
{
    // AddAndSaveEventAsync asserts a non-null OrderingContext.GetCurrentTransaction(), which is
    // only ever populated by a real relational BeginTransactionAsync call - unsupported by the
    // InMemory provider, same limitation already documented for TransactionBehavior. Only the
    // publish loop is covered here.
    [TestClass]
    public class OrderingIntegrationEventServiceTests
    {
        // OrderingContext's DbSet<T> properties are `required`, clobbered by a normal
        // object-initializer `new` call site - constructed via reflection instead, same
        // technique as Ordering.Infrastructure.UnitTests' TestOrderingContext. Only used
        // here to satisfy the constructor; the method under test never touches it.
        private static OrderingContext CreateUnusedContext()
        {
            var options = new DbContextOptionsBuilder<OrderingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return (OrderingContext)Activator.CreateInstance(typeof(OrderingContext), options, Substitute.For<IMediator>())!;
        }

        [TestMethod]
        public async Task PublishEventsThroughEventBusAsync_publishes_and_marks_each_pending_event_as_published()
        {
            var integrationEvent = new OrderStatusChangedToCancelledIntegrationEvent(1, OrderStatus.Cancelled, "Alice", "guid-1");
            // RetrieveEventLogsPendingToPublishAsync's real query projection calls this itself
            // (see the comment on OrderingIntegrationEventService) - replicated here since the
            // mocked repository can't run that projection.
            var logEntry = new IntegrationEventLogEntry(integrationEvent, Guid.NewGuid())
                .DeserializeJsonContent(typeof(OrderStatusChangedToCancelledIntegrationEvent));
            var eventLogService = Substitute.For<IIntegrationEventLogService>();
            eventLogService.RetrieveEventLogsPendingToPublishAsync(Arg.Any<Guid>()).Returns([logEntry]);
            var eventBus = Substitute.For<IEventBus>();
            var service = new OrderingIntegrationEventService(
                eventBus, CreateUnusedContext(), eventLogService, Substitute.For<ILogger<OrderingIntegrationEventService>>());

            await service.PublishEventsThroughEventBusAsync(Guid.NewGuid());

            await eventBus.Received(1).PublishAsync(Arg.Is<OrderStatusChangedToCancelledIntegrationEvent>(c =>
                c.OrderId == 1 && c.BuyerIdentityGuid == "guid-1"));
            await eventLogService.Received(1).MarkEventAsInProgressAsync(logEntry.EventId);
            await eventLogService.Received(1).MarkEventAsPublishedAsync(logEntry.EventId);
            await eventLogService.DidNotReceive().MarkEventAsFailedAsync(Arg.Any<Guid>());
        }

        [TestMethod]
        public async Task PublishEventsThroughEventBusAsync_marks_the_event_as_failed_when_the_bus_throws()
        {
            var integrationEvent = new OrderStatusChangedToCancelledIntegrationEvent(1, OrderStatus.Cancelled, "Alice", "guid-1");
            var logEntry = new IntegrationEventLogEntry(integrationEvent, Guid.NewGuid())
                .DeserializeJsonContent(typeof(OrderStatusChangedToCancelledIntegrationEvent));
            var eventLogService = Substitute.For<IIntegrationEventLogService>();
            eventLogService.RetrieveEventLogsPendingToPublishAsync(Arg.Any<Guid>()).Returns([logEntry]);
            var eventBus = Substitute.For<IEventBus>();
            eventBus.PublishAsync(Arg.Any<IntegrationEvent>()).Returns(Task.FromException(new InvalidOperationException("boom")));
            var service = new OrderingIntegrationEventService(
                eventBus, CreateUnusedContext(), eventLogService, Substitute.For<ILogger<OrderingIntegrationEventService>>());

            await service.PublishEventsThroughEventBusAsync(Guid.NewGuid());

            await eventLogService.Received(1).MarkEventAsFailedAsync(logEntry.EventId);
            await eventLogService.DidNotReceive().MarkEventAsPublishedAsync(Arg.Any<Guid>());
        }
    }
}
