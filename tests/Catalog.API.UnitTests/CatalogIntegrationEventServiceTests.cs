using eShop.Catalog.API.IntegrationEvents;
using eShop.Catalog.API.IntegrationEvents.Events;
using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.IntegrationEventLogEF.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace eShop.Catalog.API.UnitTests;

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
}
