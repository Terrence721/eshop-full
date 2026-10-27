using System.Text.Json;
using eShop.Basket.API.Model;
using eShop.Basket.API.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using StackExchange.Redis;

namespace eShop.Basket.API.UnitTests
{
    [TestClass]
    public class RedisBasketRepositoryTests
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private static RedisBasketRepository CreateRepository(IDatabase database, ILogger<RedisBasketRepository>? logger = null)
        {
            var redis = Substitute.For<IConnectionMultiplexer>();
            redis.GetDatabase().Returns(database);
            return new RedisBasketRepository(logger ?? Substitute.For<ILogger<RedisBasketRepository>>(), redis);
        }

        private static Lease<byte> CreateLease(CustomerBasket basket)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(basket, JsonOptions);
            var lease = Lease<byte>.Create(json.Length, false);
            json.CopyTo(lease.Span);
            return lease;
        }

        private static Lease<byte> CreateLease(ReadOnlySpan<byte> bytes)
        {
            var lease = Lease<byte>.Create(bytes.Length, false);
            bytes.CopyTo(lease.Span);
            return lease;
        }

        [TestMethod]
        public void Constructor_uses_the_database_from_the_multiplexer()
        {
            var database = Substitute.For<IDatabase>();
            var redis = Substitute.For<IConnectionMultiplexer>();
            redis.GetDatabase().Returns(database);

            var repository = new RedisBasketRepository(Substitute.For<ILogger<RedisBasketRepository>>(), redis);

            Assert.IsNotNull(repository);
            redis.Received(1).GetDatabase();
        }

        [TestMethod]
        public async Task DeleteBasketAsync_returns_true_when_the_key_exists()
        {
            var database = Substitute.For<IDatabase>();
            database.KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(true);

            var result = await CreateRepository(database).DeleteBasketAsync("alice");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task DeleteBasketAsync_returns_false_when_the_key_is_missing()
        {
            var database = Substitute.For<IDatabase>();
            database.KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(false);

            var result = await CreateRepository(database).DeleteBasketAsync("alice");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task DeleteBasketAsync_passes_the_prefixed_key_to_redis()
        {
            var database = Substitute.For<IDatabase>();
            database.KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(true);

            await CreateRepository(database).DeleteBasketAsync("alice");

            await database.Received(1).KeyDeleteAsync(Arg.Is<RedisKey>(key => key.ToString() == "/basket/alice"), Arg.Any<CommandFlags>());
        }

        [TestMethod]
        public async Task DeleteBasketAsync_propagates_exceptions_from_redis()
        {
            var database = Substitute.For<IDatabase>();
            database.KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns<Task<bool>>(_ => throw new InvalidOperationException("redis down"));

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => CreateRepository(database).DeleteBasketAsync("alice"));
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
        public async Task GetBasketAsync_returns_null_when_the_lease_has_zero_length()
        {
            var empty = CreateLease(Array.Empty<byte>());
            var database = Substitute.For<IDatabase>();
            database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(empty);

            var result = await CreateRepository(database).GetBasketAsync("alice");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetBasketAsync_returns_the_deserialized_basket_when_present()
        {
            var basket = new CustomerBasket("alice") { Items = [new BasketItem { ProductId = 1, Quantity = 2, ProductName = "Widget", UnitPrice = 9.99m }] };

            var database = Substitute.For<IDatabase>();
            database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(CreateLease(basket));

            var result = await CreateRepository(database).GetBasketAsync("alice");

            Assert.IsNotNull(result);
            Assert.AreEqual("alice", result.BuyerId);
            Assert.HasCount(1, result.Items);
            Assert.AreEqual(1, result.Items[0].ProductId);
            Assert.AreEqual("Widget", result.Items[0].ProductName);
            Assert.AreEqual(9.99m, result.Items[0].UnitPrice);
            Assert.AreEqual(2, result.Items[0].Quantity);
        }

        [TestMethod]
        public async Task GetBasketAsync_deserializes_a_basket_with_no_items()
        {
            var basket = new CustomerBasket("bob");

            var database = Substitute.For<IDatabase>();
            database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(CreateLease(basket));

            var result = await CreateRepository(database).GetBasketAsync("bob");

            Assert.IsNotNull(result);
            Assert.AreEqual("bob", result.BuyerId);
            Assert.IsEmpty(result.Items);
        }

        [TestMethod]
        public async Task GetBasketAsync_throws_when_the_stored_json_is_malformed()
        {
            var database = Substitute.For<IDatabase>();
            database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(CreateLease("{not json"u8));

            await Assert.ThrowsExactlyAsync<JsonException>(() => CreateRepository(database).GetBasketAsync("alice"));
        }

        [TestMethod]
        public async Task GetBasketAsync_passes_the_prefixed_key_to_redis()
        {
            var basket = new CustomerBasket("alice");

            var database = Substitute.For<IDatabase>();
            database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(CreateLease(basket));

            await CreateRepository(database).GetBasketAsync("alice");

            await database.Received(1).StringGetLeaseAsync(Arg.Is<RedisKey>(key => key.ToString() == "/basket/alice"), Arg.Any<CommandFlags>());
        }

        [TestMethod]
        public async Task GetBasketAsync_propagates_exceptions_from_redis()
        {
            var database = Substitute.For<IDatabase>();
            database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns<Lease<byte>?>(_ => throw new InvalidOperationException("redis down"));

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => CreateRepository(database).GetBasketAsync("alice"));
        }

        [TestMethod]
        public async Task UpdateBasketAsync_throws_when_BuyerId_is_null()
        {
            var database = Substitute.For<IDatabase>();

            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => CreateRepository(database).UpdateBasketAsync(new CustomerBasket()));
        }

        [TestMethod]
        public async Task UpdateBasketAsync_throws_when_BuyerId_is_empty()
        {
            var database = Substitute.For<IDatabase>();

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => CreateRepository(database).UpdateBasketAsync(new CustomerBasket("")));
        }

        [TestMethod]
        public async Task UpdateBasketAsync_does_not_throw_for_whitespace_BuyerId()
        {
            var basket = new CustomerBasket("   ") { Items = [new BasketItem { ProductId = 1, Quantity = 1 }] };

            var database = Substitute.For<IDatabase>();
            database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<Expiration>(), Arg.Any<ValueCondition>(), Arg.Any<CommandFlags>()).Returns(true);
            database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(CreateLease(basket));

            var result = await CreateRepository(database).UpdateBasketAsync(basket);

            Assert.IsNotNull(result);
            Assert.AreEqual("   ", result.BuyerId);
        }

        [TestMethod]
        public async Task UpdateBasketAsync_returns_null_when_the_set_fails()
        {
            var database = Substitute.For<IDatabase>();
            database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<Expiration>(), Arg.Any<ValueCondition>(), Arg.Any<CommandFlags>()).Returns(false);

            var result = await CreateRepository(database).UpdateBasketAsync(new CustomerBasket("alice"));

            Assert.IsNull(result);
            await database.DidNotReceive().StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>());
        }

        [TestMethod]
        public async Task UpdateBasketAsync_does_not_reread_when_the_set_fails()
        {
            var database = Substitute.For<IDatabase>();
            database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<Expiration>(), Arg.Any<ValueCondition>(), Arg.Any<CommandFlags>()).Returns(false);

            await CreateRepository(database).UpdateBasketAsync(new CustomerBasket("alice"));

            await database.DidNotReceive().StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>());
        }

        [TestMethod]
        public async Task UpdateBasketAsync_returns_the_persisted_basket_when_the_set_succeeds()
        {
            var basket = new CustomerBasket("alice") { Items = [new BasketItem { ProductId = 7, Quantity = 3 }] };

            var database = Substitute.For<IDatabase>();
            database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<Expiration>(), Arg.Any<ValueCondition>(), Arg.Any<CommandFlags>()).Returns(true);
            database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(CreateLease(basket));

            var result = await CreateRepository(database).UpdateBasketAsync(basket);

            Assert.IsNotNull(result);
            Assert.AreEqual("alice", result.BuyerId);
            Assert.HasCount(1, result.Items);
            Assert.AreEqual(7, result.Items[0].ProductId);
            Assert.AreEqual(3, result.Items[0].Quantity);
        }

        [TestMethod]
        public async Task UpdateBasketAsync_returns_null_when_rereading_returns_nothing()
        {
            var basket = new CustomerBasket("alice");

            var database = Substitute.For<IDatabase>();
            database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<Expiration>(), Arg.Any<ValueCondition>(), Arg.Any<CommandFlags>()).Returns(true);
            database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns((Lease<byte>?)null);

            var result = await CreateRepository(database).UpdateBasketAsync(basket);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task UpdateBasketAsync_serializes_the_basket_to_the_prefixed_key()
        {
            var basket = new CustomerBasket("alice") { Items = [new BasketItem { ProductId = 7, Quantity = 3 }] };

            var database = Substitute.For<IDatabase>();
            database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<Expiration>(), Arg.Any<ValueCondition>(), Arg.Any<CommandFlags>()).Returns(true);
            database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(CreateLease(basket));

            await CreateRepository(database).UpdateBasketAsync(basket);

            await database.Received(1).StringSetAsync(Arg.Is<RedisKey>(key => key.ToString() == "/basket/alice"), Arg.Any<RedisValue>(), Arg.Any<Expiration>(), Arg.Any<ValueCondition>(), Arg.Any<CommandFlags>());
        }

        [TestMethod]
        public async Task UpdateBasketAsync_propagates_exceptions_from_string_set()
        {
            var database = Substitute.For<IDatabase>();
            database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<Expiration>(), Arg.Any<ValueCondition>(), Arg.Any<CommandFlags>()).Returns<Task<bool>>(_ => throw new InvalidOperationException("redis down"));

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => CreateRepository(database).UpdateBasketAsync(new CustomerBasket("alice")));
        }

        [TestMethod]
        public async Task UpdateBasketAsync_propagates_exceptions_from_rereading()
        {
            var basket = new CustomerBasket("alice");

            var database = Substitute.For<IDatabase>();
            database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<Expiration>(), Arg.Any<ValueCondition>(), Arg.Any<CommandFlags>()).Returns(true);
            database.StringGetLeaseAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns<Lease<byte>?>(_ => throw new InvalidOperationException("redis down"));

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => CreateRepository(database).UpdateBasketAsync(basket));
        }

        [TestMethod]
        public async Task UpdateBasketAsync_throws_when_the_basket_is_null()
        {
            var database = Substitute.For<IDatabase>();

            await Assert.ThrowsExactlyAsync<NullReferenceException>(() => CreateRepository(database).UpdateBasketAsync(null!));
        }
    }
}
