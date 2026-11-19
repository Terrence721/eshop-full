using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;
using eShop.EventBus.Events;
using eShop.IntegrationEventLogEF.Services;

namespace eShop.IntegrationEventLogEF.UnitTests.Services
{
    [TestClass]
    public class IntegrationEventLogServiceTests
    {
        [TestMethod]
        public void Dispose_does_not_throw()
        {
            var service = new IntegrationEventLogService<TestDbContext>(CreateContext());

            // Should not throw
            service.Dispose();
        }

        [TestMethod]
        public void Dispose_is_idempotent()
        {
            var service = new IntegrationEventLogService<TestDbContext>(CreateContext());

            service.Dispose();
            service.Dispose(); // Should not throw on second call
        }

        [TestMethod]
        public void Dispose_is_idempotent_multiple_times()
        {
            var service = new IntegrationEventLogService<TestDbContext>(CreateContext());

            service.Dispose();
            service.Dispose();
            service.Dispose(); // Three calls should still not throw
        }

        [TestMethod]
        public async Task SaveEventAsync_throws_when_transaction_is_null()
        {
            using var context = CreateContext();
            var service = new IntegrationEventLogService<TestDbContext>(context);

            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                service.SaveEventAsync(new TestEventAlphaIntegrationEvent(), null!));
        }

        [TestMethod]
        public async Task MarkEventAsPublishedAsync_sets_state_to_Published()
        {
            using var context = CreateContext();
            var service = new IntegrationEventLogService<TestDbContext>(context);
            var entry = AddPendingEntry(context);
            await context.SaveChangesAsync();

            await service.MarkEventAsPublishedAsync(entry.EventId);

            var updated = await context.IntegrationEventLog.SingleAsync(e => e.EventId == entry.EventId);
            Assert.AreEqual(EventState.Published, updated.State);
            Assert.AreEqual(0, updated.TimesSent);
        }

        [TestMethod]
        public async Task MarkEventAsInProgressAsync_sets_state_to_InProgress_and_increments_TimesSent()
        {
            using var context = CreateContext();
            var service = new IntegrationEventLogService<TestDbContext>(context);
            var entry = AddPendingEntry(context);
            await context.SaveChangesAsync();

            await service.MarkEventAsInProgressAsync(entry.EventId);

            var updated = await context.IntegrationEventLog.SingleAsync(e => e.EventId == entry.EventId);
            Assert.AreEqual(EventState.InProgress, updated.State);
            Assert.AreEqual(1, updated.TimesSent);
        }

        [TestMethod]
        public async Task MarkEventAsFailedAsync_sets_state_to_PublishedFailed()
        {
            using var context = CreateContext();
            var service = new IntegrationEventLogService<TestDbContext>(context);
            var entry = AddPendingEntry(context);
            await context.SaveChangesAsync();

            await service.MarkEventAsFailedAsync(entry.EventId);

            var updated = await context.IntegrationEventLog.SingleAsync(e => e.EventId == entry.EventId);
            Assert.AreEqual(EventState.PublishedFailed, updated.State);
        }

        [TestMethod]
        public async Task MarkEventAsInProgressAsync_throws_when_event_does_not_exist()
        {
            using var context = CreateContext();
            var service = new IntegrationEventLogService<TestDbContext>(context);

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
                service.MarkEventAsInProgressAsync(Guid.NewGuid()));
        }

        [TestMethod]
        public async Task RetrieveEventLogsPendingToPublishAsync_returns_empty_when_no_entries_exist()
        {
            using var context = CreateContext();
            var service = new IntegrationEventLogService<TestDbContext>(context);

            var result = await service.RetrieveEventLogsPendingToPublishAsync(Guid.NewGuid());

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public async Task RetrieveEventLogsPendingToPublishAsync_returns_only_NotPublished_entries_for_the_transaction()
        {
            using var context = CreateContext();
            var service = new IntegrationEventLogService<TestDbContext>(context);

            var transactionId = Guid.NewGuid();
            var otherTransactionId = Guid.NewGuid();

            var pending = AddPendingEntry(context, transactionId);
            var alsoPending = AddPendingEntry(context, transactionId);

            var alreadyPublished = AddPendingEntry(context, transactionId);
            alreadyPublished.State = EventState.Published;

            var failed = AddPendingEntry(context, transactionId);
            failed.State = EventState.PublishedFailed;

            // Different transaction -- must not leak into the result.
            AddPendingEntry(context, otherTransactionId);

            await context.SaveChangesAsync();

            var result = await service.RetrieveEventLogsPendingToPublishAsync(transactionId);

            CollectionAssert.AreEquivalent(
                new[] { pending.EventId, alsoPending.EventId },
                result.Select(e => e.EventId).ToArray());
        }

        [TestMethod]
        public async Task RetrieveEventLogsPendingToPublishAsync_orders_entries_by_CreationTime()
        {
            using var context = CreateContext();
            var service = new IntegrationEventLogService<TestDbContext>(context);

            var transactionId = Guid.NewGuid();
            var later = AddPendingEntry(context, transactionId, new DateTime(2024, 6, 2, 0, 0, 0, DateTimeKind.Utc));
            var earlier = AddPendingEntry(context, transactionId, new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));
            await context.SaveChangesAsync();

            var result = await service.RetrieveEventLogsPendingToPublishAsync(transactionId);

            Assert.HasCount(2, result);
            Assert.AreEqual(earlier.EventId, result[0].EventId);
            Assert.AreEqual(later.EventId, result[1].EventId);
        }

        [TestMethod]
        public async Task RetrieveEventLogsPendingToPublishAsync_deserializes_the_event_content()
        {
            using var context = CreateContext();
            var service = new IntegrationEventLogService<TestDbContext>(context);

            var transactionId = Guid.NewGuid();
            var original = new TestEventAlphaIntegrationEvent { Message = "hello" };
            context.IntegrationEventLog.Add(new IntegrationEventLogEntry(original, transactionId));
            await context.SaveChangesAsync();

            var result = await service.RetrieveEventLogsPendingToPublishAsync(transactionId);

            Assert.HasCount(1, result);
            var deserialized = result[0].IntegrationEvent as TestEventAlphaIntegrationEvent;
            Assert.IsNotNull(deserialized);
            Assert.AreEqual("hello", deserialized!.Message);
            Assert.AreEqual(original.Id, deserialized.Id);
        }

        private static IntegrationEventLogEntry AddPendingEntry(
            TestDbContext context,
            Guid? transactionId = null,
            DateTime? creationTime = null)
        {
            if (creationTime is not null)
            {
                var backdatedEvent = new BackdatedIntegrationEvent(creationTime.Value);
                var entry = new IntegrationEventLogEntry(backdatedEvent, transactionId ?? Guid.NewGuid());
                context.IntegrationEventLog.Add(entry);
                return entry;
            }
            else
            {
                var entry = new IntegrationEventLogEntry(new TestEventAlphaIntegrationEvent(), transactionId ?? Guid.NewGuid());
                context.IntegrationEventLog.Add(entry);
                return entry;
            }
        }

        private static TestDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new TestDbContext(options);
        }

        private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
        {
            public DbSet<IntegrationEventLogEntry> IntegrationEventLog => Set<IntegrationEventLogEntry>();

            protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.UseIntegrationEventLogs();
        }
    }
}

// Top-level test events (must be public and end with "IntegrationEvent" for the resolver to find them)
namespace eShop.IntegrationEventLogEF.UnitTests.Services
{
    public sealed record TestEventAlphaIntegrationEvent : IntegrationEvent
    {
        public string? Message { get; init; }
    }

    public sealed record TestEventBetaIntegrationEvent : IntegrationEvent
    {
        public string? Message { get; init; }
    }

    public sealed record BackdatedIntegrationEvent : IntegrationEvent
    {
        public BackdatedIntegrationEvent(DateTime creationDate)
        {
            CreationDate = creationDate;
        }
    }
}