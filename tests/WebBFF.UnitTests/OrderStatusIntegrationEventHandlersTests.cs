using eShop.WebBFF.IntegrationEvents.EventHandling;
using eShop.WebBFF.IntegrationEvents.Events;
using eShop.WebBFF.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.WebBFF.UnitTests
{
    [TestClass]
    public class OrderStatusIntegrationEventHandlersTests
    {
        [TestMethod]
        public async Task Submitted_handler_notifies_order_status_changed()
        {
            var notifications = new OrderStatusNotifications();
            bool notified = false;
            using var _ = notifications.Subscribe("buyer-1", () => { notified = true; return Task.CompletedTask; });

            var handler = new OrderStatusChangedToSubmittedIntegrationEventHandler(notifications, Substitute.For<ILogger<OrderStatusChangedToSubmittedIntegrationEventHandler>>());
            await handler.Handle(new OrderStatusChangedToSubmittedIntegrationEvent(1, "Submitted", "Alice", "buyer-1"));

            Assert.IsTrue(notified);
        }

        [TestMethod]
        public async Task AwaitingValidation_handler_notifies_order_status_changed()
        {
            var notifications = new OrderStatusNotifications();
            bool notified = false;
            using var _ = notifications.Subscribe("buyer-2", () => { notified = true; return Task.CompletedTask; });

            var handler = new OrderStatusChangedToAwaitingValidationIntegrationEventHandler(notifications, Substitute.For<ILogger<OrderStatusChangedToAwaitingValidationIntegrationEventHandler>>());
            await handler.Handle(new OrderStatusChangedToAwaitingValidationIntegrationEvent(1, "AwaitingValidation", "Bob", "buyer-2"));

            Assert.IsTrue(notified);
        }

        [TestMethod]
        public async Task Paid_handler_notifies_order_status_changed()
        {
            var notifications = new OrderStatusNotifications();
            bool notified = false;
            using var _ = notifications.Subscribe("buyer-3", () => { notified = true; return Task.CompletedTask; });

            var handler = new OrderStatusChangedToPaidIntegrationEventHandler(notifications, Substitute.For<ILogger<OrderStatusChangedToPaidIntegrationEventHandler>>());
            await handler.Handle(new OrderStatusChangedToPaidIntegrationEvent(1, "Paid", "Carol", "buyer-3"));

            Assert.IsTrue(notified);
        }

        [TestMethod]
        public async Task StockConfirmed_handler_notifies_order_status_changed()
        {
            var notifications = new OrderStatusNotifications();
            bool notified = false;
            using var _ = notifications.Subscribe("buyer-4", () => { notified = true; return Task.CompletedTask; });

            var handler = new OrderStatusChangedToStockConfirmedIntegrationEventHandler(notifications, Substitute.For<ILogger<OrderStatusChangedToStockConfirmedIntegrationEventHandler>>());
            await handler.Handle(new OrderStatusChangedToStockConfirmedIntegrationEvent(1, "StockConfirmed", "Dave", "buyer-4"));

            Assert.IsTrue(notified);
        }

        [TestMethod]
        public async Task Shipped_handler_notifies_order_status_changed()
        {
            var notifications = new OrderStatusNotifications();
            bool notified = false;
            using var _ = notifications.Subscribe("buyer-5", () => { notified = true; return Task.CompletedTask; });

            var handler = new OrderStatusChangedToShippedIntegrationEventHandler(notifications, Substitute.For<ILogger<OrderStatusChangedToShippedIntegrationEventHandler>>());
            await handler.Handle(new OrderStatusChangedToShippedIntegrationEvent(1, "Shipped", "Eve", "buyer-5"));

            Assert.IsTrue(notified);
        }

        [TestMethod]
        public async Task Cancelled_handler_notifies_order_status_changed()
        {
            var notifications = new OrderStatusNotifications();
            bool notified = false;
            using var _ = notifications.Subscribe("buyer-6", () => { notified = true; return Task.CompletedTask; });

            var handler = new OrderStatusChangedToCancelledIntegrationEventHandler(notifications, Substitute.For<ILogger<OrderStatusChangedToCancelledIntegrationEventHandler>>());
            await handler.Handle(new OrderStatusChangedToCancelledIntegrationEvent(1, "Cancelled", "Frank", "buyer-6"));

            Assert.IsTrue(notified);
        }
    }
}
