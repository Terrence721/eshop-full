using eShop.Catalog.API.IntegrationEvents;
using eShop.Catalog.API.IntegrationEvents.EventHandling;
using eShop.Catalog.API.IntegrationEvents.Events;
using eShop.Catalog.API.Model;
using eShop.EventBus.Events;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Catalog.API.UnitTests;

[TestClass]
public class OrderStatusChangedToAwaitingValidationIntegrationEventHandlerTests
{
    private static OrderStatusChangedToAwaitingValidationIntegrationEventHandler CreateHandler(
        eShop.Catalog.API.Infrastructure.CatalogContext context, ICatalogIntegrationEventService catalogIntegrationEventService)
        => new(context, catalogIntegrationEventService, Substitute.For<ILogger<OrderStatusChangedToAwaitingValidationIntegrationEventHandler>>());

    // Captures whatever event the handler actually published/saved, so
    // assertions can use ordinary C# (is-pattern, LINQ) instead of an
    // Arg.Is expression tree, which can't contain is-pattern matching at all.
    private static async Task<IntegrationEvent> HandleAndCapturePublishedEvent(
        eShop.Catalog.API.Infrastructure.CatalogContext context, OrderStatusChangedToAwaitingValidationIntegrationEvent @event)
    {
        IntegrationEvent? published = null;
        var service = Substitute.For<ICatalogIntegrationEventService>();
        _ = service.PublishThroughEventBusAsync(Arg.Do<IntegrationEvent>(e => published = e));
        var handler = CreateHandler(context, service);

        await handler.Handle(@event);

        await service.Received(1).SaveEventAndCatalogContextChangesAsync(published!);
        return published!;
    }

    [TestMethod]
    public async Task Handle_publishes_confirmed_when_every_item_has_enough_stock()
    {
        using var context = TestCatalogContext.Create();
        context.CatalogItems.Add(new CatalogItem("Widget") { Id = 1, AvailableStock = 10 });
        context.CatalogItems.Add(new CatalogItem("Gadget") { Id = 2, AvailableStock = 5 });
        await context.SaveChangesAsync();
        var @event = new OrderStatusChangedToAwaitingValidationIntegrationEvent(100, [new OrderStockItem(1, 3), new OrderStockItem(2, 5)]);

        var published = await HandleAndCapturePublishedEvent(context, @event);

        var confirmed = (OrderStockConfirmedIntegrationEvent)published;
        Assert.AreEqual(100, confirmed.OrderId);
    }

    [TestMethod]
    public async Task Handle_publishes_rejected_with_per_item_stock_flags_when_any_item_is_short()
    {
        using var context = TestCatalogContext.Create();
        context.CatalogItems.Add(new CatalogItem("Widget") { Id = 1, AvailableStock = 10 });
        context.CatalogItems.Add(new CatalogItem("Gadget") { Id = 2, AvailableStock = 2 });
        await context.SaveChangesAsync();
        var @event = new OrderStatusChangedToAwaitingValidationIntegrationEvent(100, [new OrderStockItem(1, 3), new OrderStockItem(2, 5)]);

        var published = await HandleAndCapturePublishedEvent(context, @event);

        var rejected = (OrderStockRejectedIntegrationEvent)published;
        Assert.AreEqual(100, rejected.OrderId);
        Assert.AreEqual(2, rejected.OrderStockItems.Count);
        Assert.IsTrue(rejected.OrderStockItems.Single(i => i.ProductId == 1).HasStock);
        Assert.IsFalse(rejected.OrderStockItems.Single(i => i.ProductId == 2).HasStock);
    }

    [TestMethod]
    public async Task Handle_skips_order_items_whose_catalog_item_no_longer_exists()
    {
        using var context = TestCatalogContext.Create();
        context.CatalogItems.Add(new CatalogItem("Widget") { Id = 1, AvailableStock = 10 });
        await context.SaveChangesAsync();
        var @event = new OrderStatusChangedToAwaitingValidationIntegrationEvent(100, [new OrderStockItem(1, 3), new OrderStockItem(999, 1)]);

        var published = await HandleAndCapturePublishedEvent(context, @event);

        // Only the real item (id 1, enough stock) is considered -- the missing
        // product (999) is silently skipped, same as upstream's own behavior.
        var confirmed = (OrderStockConfirmedIntegrationEvent)published;
        Assert.AreEqual(100, confirmed.OrderId);
    }
}
