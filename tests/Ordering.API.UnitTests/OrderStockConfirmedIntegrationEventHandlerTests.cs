using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class OrderStockConfirmedIntegrationEventHandlerTests
{
    [TestMethod]
    public async Task Handle_sends_a_SetStockConfirmedOrderStatusCommand_for_the_order()
    {
        var mediator = Substitute.For<IMediator>();
        var handler = new OrderStockConfirmedIntegrationEventHandler(
            mediator, Substitute.For<ILogger<OrderStockConfirmedIntegrationEventHandler>>());

        await handler.Handle(new OrderStockConfirmedIntegrationEvent(orderId: 5));

        await mediator.Received(1).Send(Arg.Is<SetStockConfirmedOrderStatusCommand>(c => c.OrderNumber == 5));
    }
}
