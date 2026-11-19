using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.PaymentProcessor;
using eShop.PaymentProcessor.IntegrationEvents.EventHandling;
using eShop.PaymentProcessor.IntegrationEvents.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace eShop.PaymentProcessor.UnitTests
{
    [TestClass]
    public class OrderStatusChangedToStockConfirmedIntegrationEventHandlerEdgeCaseTests
    {
        private static OrderStatusChangedToStockConfirmedIntegrationEventHandler CreateHandler(
            IEventBus eventBus,
            bool paymentSucceeded,
            ILogger<OrderStatusChangedToStockConfirmedIntegrationEventHandler>? logger = null)
        {
            var options = Substitute.For<IOptionsMonitor<PaymentOptions>>();
            options.CurrentValue.Returns(new PaymentOptions { PaymentSucceeded = paymentSucceeded });
            return new OrderStatusChangedToStockConfirmedIntegrationEventHandler(
                eventBus,
                options,
                logger ?? Substitute.For<ILogger<OrderStatusChangedToStockConfirmedIntegrationEventHandler>>());
        }

        [TestMethod]
        public async Task Handle_handles_different_order_ids()
        {
            // Large order ID should not cause overflow or special handling issues
            var eventBus = Substitute.For<IEventBus>();
            var handler = CreateHandler(eventBus, paymentSucceeded: true);

            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(int.MaxValue));

            await eventBus.Received(1).PublishAsync(Arg.Any<OrderPaymentSucceededIntegrationEvent>());
        }

        [TestMethod]
        public async Task Handle_publishes_success_event_for_even_order_id()
        {
            var eventBus = Substitute.For<IEventBus>();
            var handler = CreateHandler(eventBus, paymentSucceeded: true);

            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(100));

            await eventBus.Received(1).PublishAsync(
                Arg.Is<OrderPaymentSucceededIntegrationEvent>(e => e.OrderId == 100));
        }

        [TestMethod]
        public async Task Handle_publishes_failed_event_for_odd_order_id_with_failing_payment()
        {
            var eventBus = Substitute.For<IEventBus>();
            var handler = CreateHandler(eventBus, paymentSucceeded: false);

            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(999));

            await eventBus.Received(1).PublishAsync(
                Arg.Is<OrderPaymentFailedIntegrationEvent>(e => e.OrderId == 999));
        }

        [TestMethod]
        public void Constructor_allows_null_logger_in_dependency_injection_context()
        {
            // In real DI context, logger is injected by framework and should never be null.
            // This test documents that the constructor accepts it (via nullable annotation).
            var eventBus = Substitute.For<IEventBus>();
            var options = Substitute.For<IOptionsMonitor<PaymentOptions>>();
            options.CurrentValue.Returns(new PaymentOptions { PaymentSucceeded = true });

            // Should not throw even if logger is null in pathological cases
            var handler = new OrderStatusChangedToStockConfirmedIntegrationEventHandler(
                eventBus,
                options,
                logger: null!);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task Handle_allows_any_integration_event_type_from_bus_contract()
        {
            // The handler publishes a specific event type but the bus contract accepts IntegrationEvent base.
            // Verify the published event implements the base type correctly.
            var eventBus = Substitute.For<IEventBus>();
            var handler = CreateHandler(eventBus, paymentSucceeded: true);

            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(7));

            await eventBus.Received(1).PublishAsync(Arg.Any<IntegrationEvent>());
        }

        [TestMethod]
        public async Task Handle_completes_when_event_bus_is_not_configured_for_subsequent_calls()
        {
            // Test that calling Handle multiple times works independently
            var eventBus = Substitute.For<IEventBus>();
            var handler = CreateHandler(eventBus, paymentSucceeded: true);

            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(1));
            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(2));
            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(3));

            await eventBus.Received(3).PublishAsync(Arg.Any<OrderPaymentSucceededIntegrationEvent>());
        }
    }
}
