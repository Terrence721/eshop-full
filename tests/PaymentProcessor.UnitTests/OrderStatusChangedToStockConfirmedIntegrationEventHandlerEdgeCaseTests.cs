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
        public async Task Handle_publishes_success_event_for_large_order_id()
        {
            // Verify no overflow or special-case issues with large int values
            var eventBus = Substitute.For<IEventBus>();
            var handler = CreateHandler(eventBus, paymentSucceeded: true);

            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(int.MaxValue));

            await eventBus.Received(1).PublishAsync(Arg.Any<OrderPaymentSucceededIntegrationEvent>());
        }

        [TestMethod]
        public async Task Handle_publishes_success_event_with_correct_order_id_100()
        {
            var eventBus = Substitute.For<IEventBus>();
            var handler = CreateHandler(eventBus, paymentSucceeded: true);

            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(100));

            await eventBus.Received(1).PublishAsync(
                Arg.Is<OrderPaymentSucceededIntegrationEvent>(e => e.OrderId == 100));
        }

        [TestMethod]
        public async Task Handle_publishes_failed_event_with_correct_order_id_999()
        {
            var eventBus = Substitute.For<IEventBus>();
            var handler = CreateHandler(eventBus, paymentSucceeded: false);

            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(999));

            await eventBus.Received(1).PublishAsync(
                Arg.Is<OrderPaymentFailedIntegrationEvent>(e => e.OrderId == 999));
        }

        [TestMethod]
        public void Constructor_accepts_null_logger_argument()
        {
            // Constructor accepts a logger; passing null should not throw at construction time
            var eventBus = Substitute.For<IEventBus>();
            var options = Substitute.For<IOptionsMonitor<PaymentOptions>>();
            options.CurrentValue.Returns(new PaymentOptions { PaymentSucceeded = true });

            var handler = new OrderStatusChangedToStockConfirmedIntegrationEventHandler(
                eventBus, options, logger: null!);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task Handle_publishes_base_integration_event_type()
        {
            // Verify the published events conform to the base IntegrationEvent contract
            var eventBus = Substitute.For<IEventBus>();
            var handler = CreateHandler(eventBus, paymentSucceeded: true);

            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(7));

            await eventBus.Received(1).PublishAsync(Arg.Any<IntegrationEvent>());
        }

        [TestMethod]
        public async Task Handle_can_be_called_multiple_times_independently()
        {
            // Each Handle call should be independent — processing one event does not affect another
            var eventBus = Substitute.For<IEventBus>();
            var handler = CreateHandler(eventBus, paymentSucceeded: true);

            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(1));
            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(2));
            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(3));

            await eventBus.Received(3).PublishAsync(Arg.Any<OrderPaymentSucceededIntegrationEvent>());
        }
    }
}
