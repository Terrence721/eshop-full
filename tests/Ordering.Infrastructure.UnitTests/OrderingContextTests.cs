using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    }
}
