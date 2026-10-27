using eShop.Catalog.API.IntegrationEvents;
using eShop.Catalog.API.IntegrationEvents.Events;
using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.IntegrationEventLogEF;
using eShop.IntegrationEventLogEF.Services;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace eShop.Catalog.API.UnitTests
{
    // SaveEventAndCatalogContextChangesAsync isn't covered here: it goes through
    // ResilientTransaction.New(catalogContext).ExecuteAsync, which calls
    // Database.BeginTransactionAsync -- a relational-only API EF Core's InMemory
    // provider doesn't implement at all (throws InvalidOperationException), so
    // it can't be exercised without a real relational database.
    [TestClass]
    public class CatalogIntegrationEventServiceTests
    {
        private static CatalogIntegrationEventService CreateService(IEventBus eventBus, IIntegrationEventLogService logService)
            => new(Substitute.For<ILogger<CatalogIntegrationEventService>>(), eventBus, TestCatalogContext.Create(), logService);

        [TestMethod]
        public async Task PublishThroughEventBusAsync_marks_in_progress_publishes_then_marks_published_on_success()
        {
            var eventBus = Substitute.For<IEventBus>();
            var logService = Substitute.For<IIntegrationEventLogService>();
            var service = CreateService(eventBus, logService);
            var evt = new ProductPriceChangedIntegrationEvent(1, 9.99m, 12.99m);

            await service.PublishThroughEventBusAsync(evt);

            await logService.Received(1).MarkEventAsInProgressAsync(evt.Id);
            await eventBus.Received(1).PublishAsync(evt);
            await logService.Received(1).MarkEventAsPublishedAsync(evt.Id);
            await logService.DidNotReceive().MarkEventAsFailedAsync(Arg.Any<Guid>());
        }

        [TestMethod]
        public async Task PublishThroughEventBusAsync_marks_failed_and_swallows_the_exception_when_publishing_throws()
        {
            var eventBus = Substitute.For<IEventBus>();
            eventBus.PublishAsync(Arg.Any<IntegrationEvent>()).ThrowsAsync(new InvalidOperationException("broker unreachable"));
            var logService = Substitute.For<IIntegrationEventLogService>();
            var service = CreateService(eventBus, logService);
            var evt = new ProductPriceChangedIntegrationEvent(1, 9.99m, 12.99m);

            await service.PublishThroughEventBusAsync(evt);

            await logService.Received(1).MarkEventAsFailedAsync(evt.Id);
            await logService.DidNotReceive().MarkEventAsPublishedAsync(Arg.Any<Guid>());
        }

    [TestMethod]
    public async Task PublishThroughEventBusAsync_marks_in_progress_even_when_marking_published_throws()
    {
        var eventBus = Substitute.For<IEventBus>();
        var logService = Substitute.For<IIntegrationEventLogService>();
        logService.MarkEventAsPublishedAsync(Arg.Any<Guid>()).ThrowsAsync(new InvalidOperationException("publish log failed"));
        var service = CreateService(eventBus, logService);
        var evt = new ProductPriceChangedIntegrationEvent(1, 9.99m, 12.99m);

        await service.PublishThroughEventBusAsync(evt);

        await logService.Received(1).MarkEventAsInProgressAsync(evt.Id);
        await eventBus.Received(1).PublishAsync(evt);
        await logService.Received(1).MarkEventAsFailedAsync(evt.Id);
    }

    [TestMethod]
    public async Task PublishThroughEventBusAsync_does_not_publish_or_mark_published_when_marking_in_progress_throws()
    {
        var eventBus = Substitute.For<IEventBus>();
        var logService = Substitute.For<IIntegrationEventLogService>();
        logService.MarkEventAsInProgressAsync(Arg.Any<Guid>()).ThrowsAsync(new InvalidOperationException("in progress log failed"));
        var service = CreateService(eventBus, logService);
        var evt = new ProductPriceChangedIntegrationEvent(1, 9.99m, 12.99m);

        await service.PublishThroughEventBusAsync(evt);

        await eventBus.DidNotReceive().PublishAsync(Arg.Any<IntegrationEvent>());
        await logService.Received(1).MarkEventAsFailedAsync(evt.Id);
    }

    [TestMethod]
    public void Dispose_disposes_the_integration_event_log_service_when_it_is_disposable()
    {
        var logService = Substitute.For<IIntegrationEventLogService, IDisposable>();
        var service = CreateService(Substitute.For<IEventBus>(), logService);

        service.Dispose();

        ((IDisposable)logService).Received(1).Dispose();
    }

    [TestMethod]
    public void Dispose_is_idempotent()
    {
        var logService = Substitute.For<IIntegrationEventLogService, IDisposable>();
        var service = CreateService(Substitute.For<IEventBus>(), logService);

        service.Dispose();
        service.Dispose();

        ((IDisposable)logService).Received(1).Dispose();
    }

    [TestMethod]
    public void Dispose_does_not_throw_when_the_log_service_is_not_disposable()
    {
        var logService = new NonDisposableCatalogIntegrationEventLogService();
        var service = CreateService(Substitute.For<IEventBus>(), logService);

        service.Dispose();

        // Reaching this line proves Dispose did not throw.
    }

    private sealed class NonDisposableCatalogIntegrationEventLogService : IIntegrationEventLogService
    {
        public Task<IReadOnlyList<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId) =>
            Task.FromResult<IReadOnlyList<IntegrationEventLogEntry>>(Array.Empty<IntegrationEventLogEntry>());

        public Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction) => Task.CompletedTask;
        public Task MarkEventAsPublishedAsync(Guid eventId) => Task.CompletedTask;
        public Task MarkEventAsInProgressAsync(Guid eventId) => Task.CompletedTask;
        public Task MarkEventAsFailedAsync(Guid eventId) => Task.CompletedTask;
    }
    }
}
