using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class GracePeriodConfirmedIntegrationEventHandlerTests
{
    [TestMethod]
    public async Task Handle_sends_a_SetAwaitingValidationOrderStatusCommand_for_the_order()
    {
        var mediator = Substitute.For<IMediator>();
        var handler = new GracePeriodConfirmedIntegrationEventHandler(
            mediator, Substitute.For<ILogger<GracePeriodConfirmedIntegrationEventHandler>>());

        await handler.Handle(new GracePeriodConfirmedIntegrationEvent(orderId: 5));

        await mediator.Received(1).Send(Arg.Is<SetAwaitingValidationOrderStatusCommand>(c => c.OrderNumber == 5));
    }
}
