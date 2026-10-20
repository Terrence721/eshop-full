using eShop.WebhookClient.Services;

namespace eShop.WebhookClient.UnitTests;

[TestClass]
public class HooksRepositoryTests
{
    [TestMethod]
    public async Task AddNew_stores_the_hook_so_GetAll_returns_it()
    {
        var repository = new HooksRepository();
        var hook = new WebHookReceived { When = DateTime.UtcNow, Data = "payload", Token = "tok" };

        await repository.AddNew(hook);

        var all = (await repository.GetAll()).ToList();
        Assert.AreSame(hook, all.Single());
    }

    [TestMethod]
    public async Task AddNew_notifies_every_subscriber()
    {
        var repository = new HooksRepository();
        var firstNotified = false;
        var secondNotified = false;
        using var first = repository.Subscribe(() => { firstNotified = true; return Task.CompletedTask; });
        using var second = repository.Subscribe(() => { secondNotified = true; return Task.CompletedTask; });

        await repository.AddNew(new WebHookReceived());

        Assert.IsTrue(firstNotified);
        Assert.IsTrue(secondNotified);
    }

    [TestMethod]
    public async Task AddNew_swallows_a_subscriber_exception_and_still_notifies_the_others()
    {
        var repository = new HooksRepository();
        var otherNotified = false;
        using var failing = repository.Subscribe(() => throw new InvalidOperationException("boom"));
        using var other = repository.Subscribe(() => { otherNotified = true; return Task.CompletedTask; });

        await repository.AddNew(new WebHookReceived());

        Assert.IsTrue(otherNotified);
    }

    [TestMethod]
    public async Task Subscribe_dispose_stops_future_notifications()
    {
        var repository = new HooksRepository();
        var notifyCount = 0;
        var subscription = repository.Subscribe(() => { notifyCount++; return Task.CompletedTask; });

        subscription.Dispose();
        await repository.AddNew(new WebHookReceived());

        Assert.AreEqual(0, notifyCount);
    }
}
