using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class OrderPaymentSucceededIntegrationEventHandlerTests
{
    [TestMethod]
    public async Task Handle_sends_a_SetPaidOrderStatusCommand_for_the_order()
    {
        var mediator = Substitute.For<IMediator>();
        var handler = new OrderPaymentSucceededIntegrationEventHandler(
            mediator, Substitute.For<ILogger<OrderPaymentSucceededIntegrationEventHandler>>());

        await handler.Handle(new OrderPaymentSucceededIntegrationEvent(orderId: 5));

        await mediator.Received(1).Send(Arg.Is<SetPaidOrderStatusCommand>(c => c.OrderNumber == 5));
    }
}
