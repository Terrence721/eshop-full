using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Infrastructure.Services;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests
{
    [TestClass]
    public class CreateOrderCommandHandlerTests
    {
        private static CreateOrderCommand CreateCommand(List<BasketItem>? items = null) =>
            new(items ?? [new() { ProductId = 1, ProductName = "Widget", PictureUrl = "widget.png", UnitPrice = 10.00m, Quantity = 2 }],
                "user-1", "Alice", "Springfield", "123 Main St", "IL", "USA", "62704",
                "411111111111", "Alice", DateTime.UtcNow.AddYears(1), "123", 1);

        private static CreateOrderCommandHandler CreateHandler(
            IOrderRepository orderRepository, IOrderingIntegrationEventService integrationEventService)
        {
            return new CreateOrderCommandHandler(
                Substitute.For<IMediator>(),
                integrationEventService,
                orderRepository,
                Substitute.For<IIdentityService>(),
                Substitute.For<ILogger<CreateOrderCommandHandler>>());
        }

        [TestMethod]
        public async Task Handle_adds_the_order_and_saves()
        {
            var orderRepository = Substitute.For<IOrderRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);
            orderRepository.UnitOfWork.Returns(unitOfWork);
            var handler = CreateHandler(orderRepository, Substitute.For<IOrderingIntegrationEventService>());

            var result = await handler.Handle(CreateCommand(), CancellationToken.None);

            Assert.IsTrue(result);
            orderRepository.Received(1).Add(Arg.Is<Order>(o => o.OrderItems.Count == 1));
        }

        [TestMethod]
        public async Task Handle_adds_an_OrderStartedIntegrationEvent_before_saving()
        {
            var orderRepository = Substitute.For<IOrderRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);
            orderRepository.UnitOfWork.Returns(unitOfWork);
            var integrationEventService = Substitute.For<IOrderingIntegrationEventService>();
            var handler = CreateHandler(orderRepository, integrationEventService);

            await handler.Handle(CreateCommand(), CancellationToken.None);

            await integrationEventService.Received(1).AddAndSaveEventAsync(
                Arg.Is<OrderStartedIntegrationEvent>(e => e.UserId == "user-1"));
        }
    }
}
