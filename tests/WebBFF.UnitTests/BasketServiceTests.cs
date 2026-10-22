using eShop.Basket.API.Grpc;
using eShop.WebBFF.Services;
using Grpc.Core;
using NSubstitute;
using BasketGrpcClient = eShop.Basket.API.Grpc.Basket.BasketClient;

namespace eShop.WebBFF.UnitTests
{
    [TestClass]
    public class BasketServiceTests
    {
        private static AsyncUnaryCall<TResponse> FakeCall<TResponse>(TResponse response) =>
            new(
                Task.FromResult(response),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => [],
                () => { });

        [TestMethod]
        public async Task GetBasketAsync_maps_the_response_and_forwards_the_bearer_token()
        {
            var client = Substitute.For<BasketGrpcClient>();
            var response = new CustomerBasketResponse { Items = { new BasketItem { ProductId = 7, Quantity = 3 } } };
            client.GetBasketAsync(Arg.Any<GetBasketRequest>(), Arg.Any<CallOptions>())
                .Returns(FakeCall(response));

            var items = await new BasketService(client).GetBasketAsync("tok-123");

            Assert.HasCount(1, items);
            Assert.AreEqual(7, items.Single().ProductId);
            Assert.AreEqual(3, items.Single().Quantity);
            // Discarded, not awaited: GetBasketAsync returns AsyncUnaryCall<T>, not Task<T>,
            // and NSubstitute's Received()-mode verification returns default (null) for
            // non-Task awaitables -- awaiting it would NRE. The call itself still verifies.
            _ = client.Received(1).GetBasketAsync(
                Arg.Any<GetBasketRequest>(),
                Arg.Is<CallOptions>(o => o.Headers!.Get("Authorization")!.Value == "Bearer tok-123"));
        }

        [TestMethod]
        public async Task UpdateBasketAsync_sends_every_item_and_forwards_the_bearer_token()
        {
            var client = Substitute.For<BasketGrpcClient>();
            var response = new CustomerBasketResponse { Items = { new BasketItem { ProductId = 9, Quantity = 1 } } };
            client.UpdateBasketAsync(Arg.Any<UpdateBasketRequest>(), Arg.Any<CallOptions>())
                .Returns(FakeCall(response));

            var items = await new BasketService(client).UpdateBasketAsync("tok-456", [new BasketQuantity(9, 1)]);

            Assert.HasCount(1, items);
            Assert.AreEqual(9, items.Single().ProductId);
            _ = client.Received(1).UpdateBasketAsync(
                Arg.Is<UpdateBasketRequest>(r => r.Items.Count == 1 && r.Items[0].ProductId == 9 && r.Items[0].Quantity == 1),
                Arg.Is<CallOptions>(o => o.Headers!.Get("Authorization")!.Value == "Bearer tok-456"));
        }

        [TestMethod]
        public async Task DeleteBasketAsync_forwards_the_bearer_token()
        {
            var client = Substitute.For<BasketGrpcClient>();
            client.DeleteBasketAsync(Arg.Any<DeleteBasketRequest>(), Arg.Any<CallOptions>())
                .Returns(FakeCall(new DeleteBasketResponse()));

            await new BasketService(client).DeleteBasketAsync("tok-789");

            _ = client.Received(1).DeleteBasketAsync(
                Arg.Any<DeleteBasketRequest>(),
                Arg.Is<CallOptions>(o => o.Headers!.Get("Authorization")!.Value == "Bearer tok-789"));
        }
    }
}
