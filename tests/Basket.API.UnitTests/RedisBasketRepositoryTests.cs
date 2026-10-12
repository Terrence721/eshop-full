using System.Text.Json;
using eShop.Basket.API.Model;
using eShop.Basket.API.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using StackExchange.Redis;

namespace eShop.Basket.API.UnitTests;

[TestClass]
public class RedisBasketRepositoryTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static RedisBasketRepository CreateRepository(IDatabase database)
    {
        var redis = Substitute.For<IConnectionMultiplexer>();
        redis.GetDatabase().Returns(database);
        return new RedisBasketRepository(Substitute.For<ILogger<RedisBasketRepository>>(), redis);
    }

    private static Lease<byte> CreateLease(CustomerBasket basket)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(basket, JsonOptions);
        var lease = Lease<byte>.Create(json.Length, false);
        json.CopyTo(lease.Span);
        return lease;
    }

    [TestMethod]
    public async Task GetBasketAsync_returns_null_when_the_key_is_missing()
    {
        var database = Substitute.For<IDatabase>();
        database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns((Lease<byte>?)null);

        var result = await CreateRepository(database).GetBasketAsync("alice");

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetBasketAsync_returns_null_when_the_lease_is_empty()
    {
        var database = Substitute.For<IDatabase>();
        database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Lease<byte>.Create(0, false));

        var result = await CreateRepository(database).GetBasketAsync("alice");

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetBasketAsync_returns_the_deserialized_basket_when_present()
    {
        var basket = new CustomerBasket("alice") { Items = [new BasketItem { ProductId = 1, Quantity = 2 }] };

        var database = Substitute.For<IDatabase>();
        database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(CreateLease(basket));

        var result = await CreateRepository(database).GetBasketAsync("alice");

        Assert.IsNotNull(result);
        Assert.AreEqual("alice", result.BuyerId);
        Assert.HasCount(1, result.Items);
        Assert.AreEqual(1, result.Items[0].ProductId);
        Assert.AreEqual(2, result.Items[0].Quantity);
    }

    [TestMethod]
    public async Task UpdateBasketAsync_throws_when_BuyerId_is_null()
    {
        var database = Substitute.For<IDatabase>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => CreateRepository(database).UpdateBasketAsync(new CustomerBasket()));
    }

    [TestMethod]
    public async Task UpdateBasketAsync_throws_when_BuyerId_is_empty()
    {
        var database = Substitute.For<IDatabase>();

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => CreateRepository(database).UpdateBasketAsync(new CustomerBasket("")));
    }

    [TestMethod]
    public async Task UpdateBasketAsync_returns_null_when_the_set_fails()
    {
        var database = Substitute.For<IDatabase>();
        database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<Expiration>(), Arg.Any<ValueCondition>(), Arg.Any<CommandFlags>())
            .Returns(false);

        var result = await CreateRepository(database).UpdateBasketAsync(new CustomerBasket("alice"));

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task UpdateBasketAsync_returns_the_persisted_basket_when_the_set_succeeds()
    {
        var basket = new CustomerBasket("alice") { Items = [new BasketItem { ProductId = 7, Quantity = 3 }] };

        var database = Substitute.For<IDatabase>();
        database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<Expiration>(), Arg.Any<ValueCondition>(), Arg.Any<CommandFlags>())
            .Returns(true);
        database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(CreateLease(basket));

        var result = await CreateRepository(database).UpdateBasketAsync(basket);

        Assert.IsNotNull(result);
        Assert.AreEqual("alice", result.BuyerId);
        Assert.HasCount(1, result.Items);
    }

    [TestMethod]
    public async Task DeleteBasketAsync_returns_the_underlying_result()
    {
        var database = Substitute.For<IDatabase>();
        database.KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(true);

        var result = await CreateRepository(database).DeleteBasketAsync("alice");

        Assert.IsTrue(result);
    }
}
