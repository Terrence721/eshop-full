using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
// The query layer's own result records (Order, CardType, ...) share names with the
// domain types this file seeds with, so only the query class itself is imported here.
using OrderQueries = eShop.Ordering.API.Application.Queries.OrderQueries;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class OrderQueriesTests
{
    // Same reflection-construction technique as Ordering.Infrastructure.UnitTests'
    // TestOrderingContext - OrderingContext's DbSet<T> properties are `required`,
    // which a normal object-initializer `new` call site would clobber.
    private static OrderingContext CreateContext() =>
        (OrderingContext)Activator.CreateInstance(
            typeof(OrderingContext),
            new DbContextOptionsBuilder<OrderingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            Substitute.For<IMediator>())!;

    [TestMethod]
    public async Task GetOrderAsync_throws_when_the_order_does_not_exist()
    {
        using var context = CreateContext();
        var queries = new OrderQueries(context);

        await Assert.ThrowsExactlyAsync<KeyNotFoundException>(() => queries.GetOrderAsync(999));
    }

    [TestMethod]
    public async Task GetOrderAsync_maps_the_order_its_address_and_its_items()
    {
        using var context = CreateContext();
        var order = new Order("user-1", "Alice", new Address("123 Main St", "Springfield", "IL", "USA", "62704"),
            1, "4111111111111111", "123", "Alice", DateTime.UtcNow.AddYears(1));
        order.AddOrderItem(productId: 1, productName: "Widget", unitPrice: 10.00m, discount: 0, pictureUrl: "widget.png", units: 2);
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        var queries = new OrderQueries(context);

        var result = await queries.GetOrderAsync(order.Id);

        Assert.AreEqual(order.Id, result.OrderNumber);
        Assert.AreEqual("Springfield", result.City);
        Assert.AreEqual("62704", result.Zipcode);
        Assert.AreEqual(20.00m, result.Total);
        Assert.AreEqual("Widget", result.OrderItems.Single().ProductName);
    }

    [TestMethod]
    public async Task GetOrdersFromUserAsync_returns_only_the_orders_for_that_buyer()
    {
        using var context = CreateContext();
        var buyer = new Buyer("guid-1", "Alice");
        context.Buyers.Add(buyer);
        await context.SaveChangesAsync();
        var order = new Order("user-1", "Alice", new Address("123 Main St", "Springfield", "IL", "USA", "62704"),
            1, "4111111111111111", "123", "Alice", DateTime.UtcNow.AddYears(1), buyerId: buyer.Id);
        var otherBuyersOrder = new Order("user-2", "Bob", new Address("1 Oak St", "Lincoln", "NE", "USA", "68508"),
            1, "4111111111111111", "123", "Bob", DateTime.UtcNow.AddYears(1));
        context.Orders.AddRange(order, otherBuyersOrder);
        await context.SaveChangesAsync();
        var queries = new OrderQueries(context);

        var result = await queries.GetOrdersFromUserAsync("guid-1");

        Assert.AreEqual(order.Id, result.Single().OrderNumber);
    }

    [TestMethod]
    public async Task GetCardTypesAsync_returns_every_seeded_card_type()
    {
        using var context = CreateContext();
        context.CardTypes.AddRange(new CardType { Id = 1, Name = "Amex" }, new CardType { Id = 2, Name = "Visa" });
        await context.SaveChangesAsync();
        var queries = new OrderQueries(context);

        var result = await queries.GetCardTypesAsync();

        CollectionAssert.AreEquivalent(new[] { "Amex", "Visa" }, result.Select(c => c.Name).ToArray());
    }
}
