using eShop.Ordering.API.Application.DomainEventHandlers;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Domain.Events;
using eShop.Ordering.Domain.Seedwork;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class OrderCancelledDomainEventHandlerTests
{
    [TestMethod]
    public async Task Handle_sends_the_StatusChangedToCancelled_event_with_the_order_and_buyer_details()
    {
        var order = new Order("user-1", "Alice", new Address("123 Main St", "Springfield", "IL", "USA", "62704"),
            1, "4111111111111111", "123", "Alice", DateTime.UtcNow.AddYears(1), buyerId: 1);
        var buyer = new Buyer("guid-1", "Alice");
        var orderRepository = Substitute.For<IOrderRepository>();
        orderRepository.GetAsync(Arg.Any<int>()).Returns(order);
        var buyerRepository = Substitute.For<IBuyerRepository>();
        buyerRepository.FindByIdAsync(1).Returns(buyer);
        var integrationEventService = Substitute.For<IOrderingIntegrationEventService>();
        var handler = new OrderCancelledDomainEventHandler(
            orderRepository, Substitute.For<ILogger<OrderCancelledDomainEventHandler>>(), buyerRepository, integrationEventService);

        await handler.Handle(new OrderCancelledDomainEvent(order), CancellationToken.None);

        await integrationEventService.Received(1).AddAndSaveEventAsync(Arg.Is<OrderStatusChangedToCancelledIntegrationEvent>(e =>
            e.OrderId == order.Id && e.OrderStatus == order.OrderStatus && e.BuyerName == "Alice" && e.BuyerIdentityGuid == "guid-1"));
    }
}
