using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Domain.SeedWork;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests
{
    [TestClass]
    public class SetStockConfirmedOrderStatusCommandHandlerTests
    {
        private static Order CreateOrder() =>
            new("user-1", "Alice", new Address("123 Main St", "Springfield", "IL", "USA", "62704"),
                1, "4111111111111111", "123", "Alice", DateTime.UtcNow.AddYears(1));

        [TestMethod]
        public async Task Handle_returns_false_when_the_order_is_not_found()
        {
            var orderRepository = Substitute.For<IOrderRepository>();
            orderRepository.GetAsync(Arg.Any<int>()).Returns((Order?)null);
            var handler = new SetStockConfirmedOrderStatusCommandHandler(orderRepository);

            var result = await handler.Handle(new SetStockConfirmedOrderStatusCommand(1), CancellationToken.None);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task Handle_transitions_the_order_and_saves_when_found()
        {
            var order = CreateOrder();
            order.SetAwaitingValidationStatus();
            var orderRepository = Substitute.For<IOrderRepository>();
            orderRepository.GetAsync(1).Returns(order);
            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);
            orderRepository.UnitOfWork.Returns(unitOfWork);
            var handler = new SetStockConfirmedOrderStatusCommandHandler(orderRepository);

            var result = await handler.Handle(new SetStockConfirmedOrderStatusCommand(1), CancellationToken.None);

            Assert.IsTrue(result);
            Assert.AreEqual(OrderStatus.StockConfirmed, order.OrderStatus);
        }
    }
}
