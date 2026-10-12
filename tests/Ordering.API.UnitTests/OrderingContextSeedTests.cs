using eShop.Ordering.API.Infrastructure;
using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class OrderingContextSeedTests
{
    private static OrderingContext CreateContext() =>
        (OrderingContext)Activator.CreateInstance(
            typeof(OrderingContext),
            new DbContextOptionsBuilder<OrderingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            Substitute.For<IMediator>())!;

    [TestMethod]
    public async Task SeedAsync_adds_the_three_predefined_card_types_when_none_exist()
    {
        using var context = CreateContext();

        await new OrderingContextSeed().SeedAsync(context);

        CollectionAssert.AreEquivalent(new[] { "Amex", "Visa", "MasterCard" }, context.CardTypes.Select(c => c.Name).ToArray());
    }

    [TestMethod]
    public async Task SeedAsync_does_not_duplicate_card_types_that_already_exist()
    {
        using var context = CreateContext();
        context.CardTypes.Add(new CardType { Id = 1, Name = "Amex" });
        await context.SaveChangesAsync();

        await new OrderingContextSeed().SeedAsync(context);

        Assert.HasCount(1, context.CardTypes.ToList());
    }
}
