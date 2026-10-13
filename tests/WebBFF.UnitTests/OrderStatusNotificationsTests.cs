using eShop.WebBFF.Services;

namespace eShop.WebBFF.UnitTests;

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
    public async Task Subscribe_dispose_stops_future_notifications()
    {
        var notifications = new OrderStatusNotifications();
        var notifyCount = 0;
        var subscription = notifications.Subscribe("buyer-1", () => { notifyCount++; return Task.CompletedTask; });

        subscription.Dispose();
        await notifications.NotifyOrderStatusChangedAsync("buyer-1");

        Assert.AreEqual(0, notifyCount);
    }
}
