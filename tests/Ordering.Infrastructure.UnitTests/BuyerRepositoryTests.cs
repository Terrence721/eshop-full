using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace eShop.Ordering.Infrastructure.UnitTests;

[TestClass]
public class BuyerRepositoryTests
{
    [TestMethod]
    public void UnitOfWork_returns_the_underlying_context()
    {
        var context = TestOrderingContext.Create(Substitute.For<IMediator>());
        var repository = new BuyerRepository(context);

        Assert.AreSame(context, repository.UnitOfWork);
    }

    [TestMethod]
    public void Add_adds_a_transient_buyer_to_the_context()
    {
        var context = TestOrderingContext.Create(Substitute.For<IMediator>());
        var repository = new BuyerRepository(context);
        var buyer = new Buyer("identity-1", "Alice");

        repository.Add(buyer);

        Assert.AreEqual(EntityState.Added, context.Entry(buyer).State);
    }

    [TestMethod]
    public async Task Add_returns_the_same_buyer_without_re_adding_when_already_persisted()
    {
        var context = TestOrderingContext.Create(Substitute.For<IMediator>());
        var repository = new BuyerRepository(context);
        var buyer = new Buyer("identity-1", "Alice");
        repository.Add(buyer);
        await context.SaveChangesAsync();
        Assert.IsFalse(buyer.IsTransient(), "the buyer should have a real id once persisted");

        var result = repository.Add(buyer);

        Assert.AreSame(buyer, result);
    }

    [TestMethod]
    public async Task FindAsync_returns_the_buyer_with_a_matching_identity()
    {
        var dbName = Guid.NewGuid().ToString();
        var mediator = Substitute.For<IMediator>();
        var writeContext = TestOrderingContext.CreateNamed(dbName, mediator);
        new BuyerRepository(writeContext).Add(new Buyer("identity-1", "Alice"));
        await writeContext.SaveChangesAsync();

        var readContext = TestOrderingContext.CreateNamed(dbName, mediator);
        var found = await new BuyerRepository(readContext).FindAsync("identity-1");

        Assert.IsNotNull(found);
        Assert.AreEqual("Alice", found.Name);
    }

    [TestMethod]
    public async Task FindAsync_returns_null_when_no_buyer_matches()
    {
        var context = TestOrderingContext.Create(Substitute.For<IMediator>());
        var repository = new BuyerRepository(context);

        var found = await repository.FindAsync("no-such-identity");

        Assert.IsNull(found);
    }

    [TestMethod]
    public async Task FindByIdAsync_returns_the_buyer_with_a_matching_id()
    {
        var dbName = Guid.NewGuid().ToString();
        var mediator = Substitute.For<IMediator>();
        var writeContext = TestOrderingContext.CreateNamed(dbName, mediator);
        var buyer = new Buyer("identity-1", "Alice");
        new BuyerRepository(writeContext).Add(buyer);
        await writeContext.SaveChangesAsync();

        var readContext = TestOrderingContext.CreateNamed(dbName, mediator);
        var found = await new BuyerRepository(readContext).FindByIdAsync(buyer.Id);

        Assert.IsNotNull(found);
        Assert.AreEqual("Alice", found.Name);
    }

    [TestMethod]
    public async Task FindByIdAsync_returns_null_when_no_buyer_matches()
    {
        var context = TestOrderingContext.Create(Substitute.For<IMediator>());
        var repository = new BuyerRepository(context);

        var found = await repository.FindByIdAsync(999);

        Assert.IsNull(found);
    }

    [TestMethod]
    public void Update_marks_a_transient_buyer_as_Added_not_Modified()
    {
        // EF Core's Update() has insert-or-update semantics keyed on whether
        // the primary key looks real: a key still at its CLR default (0, for
        // a buyer that was never persisted) is treated as Added, not
        // Modified - real EF behavior, not something BuyerRepository decides.
        var context = TestOrderingContext.Create(Substitute.For<IMediator>());
        var repository = new BuyerRepository(context);
        var buyer = new Buyer("identity-1", "Alice");

        repository.Update(buyer);

        Assert.AreEqual(EntityState.Added, context.Entry(buyer).State);
    }

    [TestMethod]
    public async Task Update_marks_an_already_persisted_buyer_as_Modified()
    {
        var dbName = Guid.NewGuid().ToString();
        var mediator = Substitute.For<IMediator>();
        var writeContext = TestOrderingContext.CreateNamed(dbName, mediator);
        var buyer = new Buyer("identity-1", "Alice");
        new BuyerRepository(writeContext).Add(buyer);
        await writeContext.SaveChangesAsync();

        var updateContext = TestOrderingContext.CreateNamed(dbName, mediator);
        new BuyerRepository(updateContext).Update(buyer);

        Assert.AreEqual(EntityState.Modified, updateContext.Entry(buyer).State);
    }
}
