using eShop.IntegrationEventLogEF.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace eShop.IntegrationEventLogEF.UnitTests.Utilities
{
    public sealed class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
    }

    [TestClass]
    public class ResilientTransactionTests
    {
        private static TestDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options);

        [TestMethod]
        public void New_throws_when_context_is_null()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => ResilientTransaction.New(null!));
        }

        [TestMethod]
        public void New_returns_a_non_null_instance()
        {
            using var context = CreateContext();

            var transaction = ResilientTransaction.New(context);

            Assert.IsNotNull(transaction);
        }

        [TestMethod]
        public async Task ExecuteAsync_invokes_the_action_and_commits_the_transaction()
        {
            using var context = CreateContext();
            var transaction = ResilientTransaction.New(context);
            var invoked = false;

            await transaction.ExecuteAsync(() =>
            {
                invoked = true;
                return Task.CompletedTask;
            });

            Assert.IsTrue(invoked);
        }

        [TestMethod]
        public async Task ExecuteAsync_propagates_exceptions_thrown_by_the_action()
        {
            using var context = CreateContext();
            var transaction = ResilientTransaction.New(context);

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
                transaction.ExecuteAsync(() => throw new InvalidOperationException("boom")));
        }

        [TestMethod]
        public async Task ExecuteAsync_does_not_invoke_the_action_twice_on_success()
        {
            using var context = CreateContext();
            var transaction = ResilientTransaction.New(context);
            var invocations = 0;

            await transaction.ExecuteAsync(() =>
            {
                invocations++;
                return Task.CompletedTask;
            });

            Assert.AreEqual(1, invocations);
        }

        [TestMethod]
        public async Task ExecuteAsync_awaits_a_pending_action_before_committing()
        {
            using var context = CreateContext();
            var transaction = ResilientTransaction.New(context);
            var completedBeforeReturn = false;

            await transaction.ExecuteAsync(async () =>
            {
                await Task.Delay(10);
                completedBeforeReturn = true;
            });

            Assert.IsTrue(completedBeforeReturn);
        }

        [TestMethod]
        public async Task ExecuteAsync_throws_null_reference_exception_when_action_is_null()
        {
            using var context = CreateContext();
            var transaction = ResilientTransaction.New(context);

            await Assert.ThrowsExactlyAsync<NullReferenceException>(() => transaction.ExecuteAsync(null!));
        }

        [TestMethod]
        public async Task ExecuteAsync_can_be_invoked_repeatedly_on_the_same_instance()
        {
            using var context = CreateContext();
            var transaction = ResilientTransaction.New(context);
            var count = 0;

            await transaction.ExecuteAsync(() => { count++; return Task.CompletedTask; });
            await transaction.ExecuteAsync(() => { count++; return Task.CompletedTask; });

            Assert.AreEqual(2, count);
        }
    }
}
