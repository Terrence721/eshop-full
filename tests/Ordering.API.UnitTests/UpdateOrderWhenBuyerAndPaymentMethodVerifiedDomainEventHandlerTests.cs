using eShop.Ordering.API.Application.DomainEventHandlers;
using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Domain.Events;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandlerTests
{
    [TestMethod]
    public async Task Handle_sets_the_order_s_buyer_and_payment_ids_from_the_event()
    {
        var order = new Order("user-1", "Alice", new Address("123 Main St", "Springfield", "IL", "USA", "62704"),
            1, "4111111111111111", "123", "Alice", DateTime.UtcNow.AddYears(1));
        var buyer = new Buyer("guid-1", "Alice");
        var payment = new PaymentMethod(1, "alias", "4111111111111111", "123", "Alice", DateTime.UtcNow.AddYears(1));
        var orderRepository = Substitute.For<IOrderRepository>();
        orderRepository.GetAsync(Arg.Any<int>()).Returns(order);
        var handler = new UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler(
            orderRepository, Substitute.For<ILogger<UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler>>());

        await handler.Handle(new BuyerAndPaymentMethodVerifiedDomainEvent(buyer, payment, order.Id), CancellationToken.None);

        Assert.AreEqual(buyer.Id, order.BuyerId);
        Assert.AreEqual(payment.Id, order.PaymentId);
    }
}
