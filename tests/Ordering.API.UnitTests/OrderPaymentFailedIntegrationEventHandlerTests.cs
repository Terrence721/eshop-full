using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests
{
    [TestClass]
    public class OrderPaymentFailedIntegrationEventHandlerTests
    {
        [TestMethod]
        public async Task Handle_sends_a_CancelOrderCommand_for_the_order()
        {
            var mediator = Substitute.For<IMediator>();
            var handler = new OrderPaymentFailedIntegrationEventHandler(
                mediator, Substitute.For<ILogger<OrderPaymentFailedIntegrationEventHandler>>());

            await handler.Handle(new OrderPaymentFailedIntegrationEvent(orderId: 5));

            await mediator.Received(1).Send(Arg.Is<CancelOrderCommand>(c => c.OrderNumber == 5));
        }
    }
}
