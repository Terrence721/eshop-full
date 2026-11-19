using eShop.WebBFF.Services;

namespace eShop.WebBFF.UnitTests
{
    [TestClass]
    public class OrderStatusNotificationsTests
    {
        [TestMethod]
        public async Task NotifyOrderStatusChangedAsync_notifies_every_subscriber_for_that_buyer()
        {
            var notifications = new OrderStatusNotifications();
            var firstNotified = false;
            var secondNotified = false;
            using var first = notifications.Subscribe("buyer-1", () => { firstNotified = true; return Task.CompletedTask; });
            using var second = notifications.Subscribe("buyer-1", () => { secondNotified = true; return Task.CompletedTask; });

            await notifications.NotifyOrderStatusChangedAsync("buyer-1");

            Assert.IsTrue(firstNotified);
            Assert.IsTrue(secondNotified);
        }

        [TestMethod]
        public async Task NotifyOrderStatusChangedAsync_does_not_notify_a_different_buyers_subscribers()
        {
            var notifications = new OrderStatusNotifications();
            var otherBuyerNotified = false;
            using var subscription = notifications.Subscribe("buyer-2", () => { otherBuyerNotified = true; return Task.CompletedTask; });

            await notifications.NotifyOrderStatusChangedAsync("buyer-1");

            Assert.IsFalse(otherBuyerNotified);
        }

        [TestMethod]
        public async Task NotifyOrderStatusChangedAsync_for_a_buyer_with_no_subscribers_does_not_throw()
        {
            var notifications = new OrderStatusNotifications();

            await notifications.NotifyOrderStatusChangedAsync("no-such-buyer");
        }

        [TestMethod]
        public async Task NotifyOrderStatusChangedAsync_swallows_a_subscriber_exception_and_still_notifies_the_others()
        {
            var notifications = new OrderStatusNotifications();
            var otherNotified = false;
            using var failing = notifications.Subscribe("buyer-1", () => throw new InvalidOperationException("boom"));
            using var other = notifications.Subscribe("buyer-1", () => { otherNotified = true; return Task.CompletedTask; });

            await notifications.NotifyOrderStatusChangedAsync("buyer-1");

            Assert.IsTrue(otherNotified);
        }

        [TestMethod]
        public async Task NotifyOrderStatusChangedAsync_swallows_all_subscriber_exceptions_and_notifies_the_others()
        {
            var notifications = new OrderStatusNotifications();
            var thirdNotified = false;
            using var first = notifications.Subscribe("buyer-1", () => throw new InvalidOperationException("first boom"));
            using var second = notifications.Subscribe("buyer-1", () => throw new InvalidOperationException("second boom"));
            using var third = notifications.Subscribe("buyer-1", () => { thirdNotified = true; return Task.CompletedTask; });

            await notifications.NotifyOrderStatusChangedAsync("buyer-1");

            Assert.IsTrue(thirdNotified);
        }

        [TestMethod]
        public async Task Subscribe_dispose_stops_future_notifications()
        {
            var notifications = new OrderStatusNotifications();
            var notifyCount = 0;
            var subscription = notifications.Subscribe("buyer-1", () => { notifyCount++; return Task.CompletedTask; });

            subscription.Dispose();
            await notifications.NotifyOrderStatusChangedAsync("buyer-1");

            Assert.AreEqual(0, notifyCount);
        }

        [TestMethod]
        public async Task Subscribe_multiple_times_notifies_all_subscriptions()
        {
            var notifications = new OrderStatusNotifications();
            var counts = new int[3];
            var subscriptions = new List<IDisposable>();

            for (int i = 0; i < 3; i++)
            {
                int index = i;
                subscriptions.Add(notifications.Subscribe("buyer-1", () => { counts[index]++; return Task.CompletedTask; }));
            }

            await notifications.NotifyOrderStatusChangedAsync("buyer-1");

            CollectionAssert.AreEquivalent(new[] { 1, 1, 1 }, counts);
        }

        [TestMethod]
        public async Task Subscribe_after_add_new_does_not_receive_past_notifications()
        {
            var notifications = new OrderStatusNotifications();

            await notifications.NotifyOrderStatusChangedAsync("buyer-1");

            var notified = false;
            using var subscription = notifications.Subscribe("buyer-1", () => { notified = true; return Task.CompletedTask; });

            Assert.IsFalse(notified);
        }
    }
}
