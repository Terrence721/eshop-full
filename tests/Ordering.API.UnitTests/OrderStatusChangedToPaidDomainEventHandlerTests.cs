using eShop.Ordering.API.Application.DomainEventHandlers;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Domain.Events;
using eShop.Ordering.Domain.SeedWork;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests
{
    [TestClass]
    public class OrderStatusChangedToPaidDomainEventHandlerTests
    {
        [TestMethod]
        public async Task Handle_sends_the_integration_event_with_the_order_items_as_stock_items()
        {
            var order = new Order("user-1", "Alice", new Address("123 Main St", "Springfield", "IL", "USA", "62704"),
                1, "4111111111111111", "123", "Alice", DateTime.UtcNow.AddYears(1), buyerId: 1);
            order.AddOrderItem(productId: 10, productName: "Widget", unitPrice: 5.00m, discount: 0, pictureUrl: "widget.png", units: 3);
            var buyer = new Buyer("guid-1", "Alice");
            var orderRepository = Substitute.For<IOrderRepository>();
            orderRepository.GetAsync(Arg.Any<int>()).Returns(order);
            var buyerRepository = Substitute.For<IBuyerRepository>();
            buyerRepository.FindByIdAsync(1).Returns(buyer);
            var integrationEventService = Substitute.For<IOrderingIntegrationEventService>();
            var handler = new OrderStatusChangedToPaidDomainEventHandler(
                orderRepository, Substitute.For<ILogger<OrderStatusChangedToPaidDomainEventHandler>>(), buyerRepository, integrationEventService);

            await handler.Handle(new OrderStatusChangedToPaidDomainEvent(order.Id, order.OrderItems), CancellationToken.None);

            await integrationEventService.Received(1).AddAndSaveEventAsync(Arg.Is<OrderStatusChangedToPaidIntegrationEvent>(e =>
                e.OrderId == order.Id && e.OrderStatus == order.OrderStatus && e.BuyerName == "Alice" && e.BuyerIdentityGuid == "guid-1" &&
                e.OrderStockItems.Single().ProductId == 10 && e.OrderStockItems.Single().Units == 3));
        }
    }
}
