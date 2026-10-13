using eShop.EventBus.Abstractions;
using eShop.PaymentProcessor;
using eShop.PaymentProcessor.IntegrationEvents.EventHandling;
using eShop.PaymentProcessor.IntegrationEvents.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace eShop.PaymentProcessor.UnitTests;

[TestClass]
public class OrderStatusChangedToStockConfirmedIntegrationEventHandlerTests
{
    private static OrderStatusChangedToStockConfirmedIntegrationEventHandler CreateHandler(IEventBus eventBus, bool paymentSucceeded)
    {
        var options = Substitute.For<IOptionsMonitor<PaymentOptions>>();
        options.CurrentValue.Returns(new PaymentOptions { PaymentSucceeded = paymentSucceeded });
        return new OrderStatusChangedToStockConfirmedIntegrationEventHandler(
            eventBus, options, Substitute.For<ILogger<OrderStatusChangedToStockConfirmedIntegrationEventHandler>>());
    }

    [TestMethod]
    public async Task Handle_publishes_a_succeeded_event_when_PaymentSucceeded_is_true()
    {
        var eventBus = Substitute.For<IEventBus>();
        var handler = CreateHandler(eventBus, paymentSucceeded: true);

        await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(1));

        await eventBus.Received(1).PublishAsync(Arg.Is<OrderPaymentSucceededIntegrationEvent>(e => e.OrderId == 1));
    }

    [TestMethod]
    public async Task Handle_publishes_a_failed_event_when_PaymentSucceeded_is_false()
    {
        var eventBus = Substitute.For<IEventBus>();
        var handler = CreateHandler(eventBus, paymentSucceeded: false);

        await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(1));

        await eventBus.Received(1).PublishAsync(Arg.Is<OrderPaymentFailedIntegrationEvent>(e => e.OrderId == 1));
    }
}
