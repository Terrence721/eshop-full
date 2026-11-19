using eShop.WebhookClient.Services;

namespace eShop.WebhookClient.UnitTests
{
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

        [TestMethod]
        public async Task GetAll_returns_hooks_in_order_of_addition()
        {
            var repository = new HooksRepository();
            var hook1 = new WebHookReceived { When = DateTime.UtcNow.AddSeconds(-2), Data = "first" };
            var hook2 = new WebHookReceived { When = DateTime.UtcNow.AddSeconds(-1), Data = "second" };

            await repository.AddNew(hook1);
            await repository.AddNew(hook2);

            var all = (await repository.GetAll()).ToList();

            Assert.AreEqual(2, all.Count);
            Assert.AreSame(hook1, all[0]);
            Assert.AreSame(hook2, all[1]);
        }

        [TestMethod]
        public async Task Subscribe_multiple_times_notifies_all_subscriptions()
        {
            var repository = new HooksRepository();
            var counts = new int[3];
            var subscriptions = new List<IDisposable>();

            for (int i = 0; i < 3; i++)
            {
                int index = i;
                subscriptions.Add(repository.Subscribe(() => { counts[index]++; return Task.CompletedTask; }));
            }

            await repository.AddNew(new WebHookReceived());

            CollectionAssert.AreEquivalent(new[] { 1, 1, 1 }, counts);
        }

        [TestMethod]
        public async Task Subscribe_after_add_new_does_not_receive_past_notifications()
        {
            var repository = new HooksRepository();
            await repository.AddNew(new WebHookReceived());

            var notified = false;
            using var subscription = repository.Subscribe(() => { notified = true; return Task.CompletedTask; });

            Assert.IsFalse(notified);
        }
    }
}
