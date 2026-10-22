using eShop.Catalog.API.Infrastructure;
using eShop.Catalog.API.IntegrationEvents.EventHandling;
using eShop.Catalog.API.IntegrationEvents.Events;
using eShop.Catalog.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Catalog.API.UnitTests
{
    [TestClass]
    public class OrderStatusChangedToPaidIntegrationEventHandlerTests
    {
        [TestMethod]
        public async Task Handle_decrements_and_persists_stock_for_each_order_item()
        {
            var dbName = Guid.NewGuid().ToString();
            await using (var seedContext = TestCatalogContext.CreateNamed(dbName))
            {
                seedContext.CatalogItems.Add(new CatalogItem("Widget") { Id = 1, AvailableStock = 10 });
                await seedContext.SaveChangesAsync();
            }

            await using (var context = TestCatalogContext.CreateNamed(dbName))
            {
                var handler = new OrderStatusChangedToPaidIntegrationEventHandler(context, Substitute.For<ILogger<OrderStatusChangedToPaidIntegrationEventHandler>>());
                var @event = new OrderStatusChangedToPaidIntegrationEvent(100, [new OrderStockItem(1, 3)]);

                await handler.Handle(@event);
            }

            await using (var verifyContext = TestCatalogContext.CreateNamed(dbName))
            {
                var item = await verifyContext.CatalogItems.FindAsync(1);
                Assert.AreEqual(7, item!.AvailableStock);
            }
        }

        [TestMethod]
        public async Task Handle_ignores_order_items_whose_catalog_item_no_longer_exists()
        {
            await using var context = TestCatalogContext.CreateNamed(Guid.NewGuid().ToString());
            var handler = new OrderStatusChangedToPaidIntegrationEventHandler(context, Substitute.For<ILogger<OrderStatusChangedToPaidIntegrationEventHandler>>());
            var @event = new OrderStatusChangedToPaidIntegrationEvent(100, [new OrderStockItem(999, 3)]);

            await handler.Handle(@event);

            // No exception, nothing to assert beyond that -- the missing item is a real,
            // expected case (catalogItem?.RemoveStock(...) short-circuits via null-conditional).
        }
    }
}
