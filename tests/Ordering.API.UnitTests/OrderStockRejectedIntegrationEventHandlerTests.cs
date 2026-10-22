using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests
{
    [TestClass]
    public class OrderStockRejectedIntegrationEventHandlerTests
    {
        [TestMethod]
        public async Task Handle_sends_a_SetStockRejectedOrderStatusCommand_with_only_the_out_of_stock_product_ids()
        {
            var mediator = Substitute.For<IMediator>();
            var handler = new OrderStockRejectedIntegrationEventHandler(
                mediator, Substitute.For<ILogger<OrderStockRejectedIntegrationEventHandler>>());
            var orderStockItems = new List<ConfirmedOrderStockItem>
            {
                new(productId: 1, hasStock: true),
                new(productId: 2, hasStock: false),
                new(productId: 3, hasStock: false),
            };

            await handler.Handle(new OrderStockRejectedIntegrationEvent(orderId: 5, orderStockItems));

            await mediator.Received(1).Send(Arg.Is<SetStockRejectedOrderStatusCommand>(c =>
                c.OrderNumber == 5 && c.OrderStockItems.SequenceEqual(new[] { 2, 3 })));
        }
    }
}
