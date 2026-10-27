using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;

namespace eShop.Ordering.Infrastructure.UnitTests
{
    // BeginTransactionAsync/CommitTransactionAsync/RollbackTransaction aren't
    // covered here - they call Database.BeginTransactionAsync(), a relational-only
    // API the InMemory provider doesn't support (same exclusion already applied
    // to Catalog.API.UnitTests's CatalogIntegrationEventServiceTests).
    [TestClass]
    public class OrderingContextTests
    {
        [TestMethod]
        public void HasActiveTransaction_is_false_by_default()
        {
            var context = TestOrderingContext.Create(Substitute.For<IMediator>());

            Assert.IsFalse(context.HasActiveTransaction);
        }

        [TestMethod]
        public void GetCurrentTransaction_is_null_by_default()
        {
            var context = TestOrderingContext.Create(Substitute.For<IMediator>());

            Assert.IsNull(context.GetCurrentTransaction());
        }

        [TestMethod]
        public async Task SaveEntitiesAsync_dispatches_domain_events_before_saving()
        {
            var mediator = Substitute.For<IMediator>();
            var context = TestOrderingContext.Create(mediator);
            var buyer = new Buyer("identity-1", "Alice");
            buyer.VerifyOrAddPaymentMethod(
                1, "alias", "4111111111111111", "123", "Alice", DateTime.UtcNow.AddYears(1), orderId: 1);
            context.Add(buyer);

            var result = await context.SaveEntitiesAsync();

            Assert.IsTrue(result);
            await mediator.Received(1).Publish(Arg.Any<BuyerAndPaymentMethodVerifiedDomainEvent>(), Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task SaveEntitiesAsync_persists_the_tracked_changes()
        {
            var context = TestOrderingContext.Create(Substitute.For<IMediator>());
            context.Add(new Buyer("identity-1", "Alice"));

            await context.SaveEntitiesAsync();

            Assert.AreEqual(1, await context.Buyers.CountAsync());
        }

    [TestMethod]
    public void Constructor_with_mediator_sets_up_the_context()
    {
        var context = TestOrderingContext.Create(Substitute.For<IMediator>());

        Assert.IsNotNull(context);
        Assert.IsFalse(context.HasActiveTransaction);
    }

    [TestMethod]
    public void Constructor_throws_when_mediator_is_null()
    {
        var options = new DbContextOptionsBuilder<OrderingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        Assert.ThrowsExactly<ArgumentNullException>(() => new OrderingContext(options, null!));
    }

    private static OrderingContext CreateTransactionalContext(IMediator mediator) =>
        new(new DbContextOptionsBuilder<OrderingContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options, mediator);

    [TestMethod]
    public async Task BeginTransactionAsync_returns_a_transaction_on_the_first_call()
    {
        var context = CreateTransactionalContext(Substitute.For<IMediator>());

        var transaction = await context.BeginTransactionAsync();

        Assert.IsNotNull(transaction);
        Assert.IsTrue(context.HasActiveTransaction);
    }

    [TestMethod]
    public async Task BeginTransactionAsync_returns_null_when_a_transaction_is_already_active()
    {
        var context = CreateTransactionalContext(Substitute.For<IMediator>());
        await context.BeginTransactionAsync();

        var second = await context.BeginTransactionAsync();

        Assert.IsNull(second);
    }

    [TestMethod]
    public async Task CommitTransactionAsync_throws_when_transaction_is_null()
    {
        var context = CreateTransactionalContext(Substitute.For<IMediator>());

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => context.CommitTransactionAsync(null!));
    }

    [TestMethod]
    public async Task CommitTransactionAsync_commits_and_clears_the_current_transaction()
    {
        var context = CreateTransactionalContext(Substitute.For<IMediator>());
        var transaction = await context.BeginTransactionAsync();
        var buyer = new Buyer("identity-1", "Alice");
        context.Add(buyer);

        await context.CommitTransactionAsync(transaction!);

        Assert.IsFalse(context.HasActiveTransaction);
        Assert.IsNull(context.GetCurrentTransaction());
        Assert.AreEqual(1, await context.Buyers.CountAsync());
    }

    [TestMethod]
    public async Task RollbackTransaction_clears_the_current_transaction()
    {
        var context = CreateTransactionalContext(Substitute.For<IMediator>());
        await context.BeginTransactionAsync();

        context.RollbackTransaction();

        Assert.IsFalse(context.HasActiveTransaction);
        Assert.IsNull(context.GetCurrentTransaction());
    }

    [TestMethod]
    public void RollbackTransaction_does_not_throw_when_no_transaction_is_active()
    {
        var context = TestOrderingContext.Create(Substitute.For<IMediator>());

        context.RollbackTransaction();

        Assert.IsFalse(context.HasActiveTransaction);
    }
    }
}
