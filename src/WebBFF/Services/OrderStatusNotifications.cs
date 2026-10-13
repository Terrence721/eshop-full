using System.Collections.Concurrent;

namespace eShop.WebBFF.Services;

// Per-buyer subscription registry, same shape as WebhookClient's own HooksRepository
// (subscribe/notify/IDisposable-unsubscribe), but keyed by buyer so each connected
// browser only ever hears about its own orders changing, not every buyer's.
public class OrderStatusNotifications
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Subscription, object?>> _subscriptionsByBuyer = new();

    public Task NotifyOrderStatusChangedAsync(string buyerIdentityGuid)
    {
        if (_subscriptionsByBuyer.TryGetValue(buyerIdentityGuid, out var subscriptions))
        {
            foreach (var subscription in subscriptions.Keys)
            {
                try
                {
                    _ = subscription.NotifyAsync();
                }
                catch (Exception)
                {
                    // It's the subscriber's responsibility to report/handle any exceptions
                    // that occur during their callback
                }
            }
        }

        return Task.CompletedTask;
    }

    public IDisposable Subscribe(string buyerIdentityGuid, Func<Task> callback)
    {
        var subscriptions = _subscriptionsByBuyer.GetOrAdd(buyerIdentityGuid, _ => new ConcurrentDictionary<Subscription, object?>());
        var subscription = new Subscription(callback, buyerIdentityGuid, this);
        subscriptions.TryAdd(subscription, null);
        return subscription;
    }

    private class Subscription(Func<Task> callback, string buyerIdentityGuid, OrderStatusNotifications owner) : IDisposable
    {
        public Task NotifyAsync() => callback();

        public void Dispose()
        {
            if (owner._subscriptionsByBuyer.TryGetValue(buyerIdentityGuid, out var subscriptions))
            {
                subscriptions.TryRemove(this, out _);
            }
        }
    }
}
