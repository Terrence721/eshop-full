using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Domain.SeedWork;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class ShipOrderCommandHandlerTests
{
    private static Order CreatePaidOrder()
    {
        var order = new Order("user-1", "Alice", new Address("123 Main St", "Springfield", "IL", "USA", "62704"),
            1, "4111111111111111", "123", "Alice", DateTime.UtcNow.AddYears(1));
        order.SetAwaitingValidationStatus();
        order.SetStockConfirmedStatus();
        order.SetPaidStatus();
        return order;
    }

    [TestMethod]
    public async Task Handle_returns_false_when_the_order_is_not_found()
    {
        var orderRepository = Substitute.For<IOrderRepository>();
        orderRepository.GetAsync(Arg.Any<int>()).Returns((Order?)null);
        var handler = new ShipOrderCommandHandler(orderRepository);

        var result = await handler.Handle(new ShipOrderCommand(1), CancellationToken.None);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task Handle_ships_the_order_and_saves_when_found()
    {
        var order = CreatePaidOrder();
        var orderRepository = Substitute.For<IOrderRepository>();
        orderRepository.GetAsync(1).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);
        orderRepository.UnitOfWork.Returns(unitOfWork);
        var handler = new ShipOrderCommandHandler(orderRepository);

        var result = await handler.Handle(new ShipOrderCommand(1), CancellationToken.None);

        Assert.IsTrue(result);
        Assert.AreEqual(OrderStatus.Shipped, order.OrderStatus);
    }
}
