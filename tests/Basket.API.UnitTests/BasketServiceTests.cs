using System.Security.Claims;
using eShop.Basket.API.Grpc;
using eShop.Basket.API.Repositories;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ModelCustomerBasket = eShop.Basket.API.Model.CustomerBasket;
using ModelBasketItem = eShop.Basket.API.Model.BasketItem;

namespace eShop.Basket.API.UnitTests;

[TestClass]
public class BasketServiceTests
{
    private static ServerCallContext CreateContext(string? userId)
    {
        var httpContext = new DefaultHttpContext();
        var claims = userId is null ? [] : new[] { new Claim("sub", userId) };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        return TestServerCallContext.Create(httpContext);
    }

    private static BasketService CreateService(IBasketRepository repository) =>
        new(repository, Substitute.For<ILogger<BasketService>>());

    [TestMethod]
    public async Task GetBasket_returns_an_empty_response_when_unauthenticated()
    {
        var repository = Substitute.For<IBasketRepository>();

        var response = await CreateService(repository).GetBasket(new GetBasketRequest(), CreateContext(null));

        Assert.IsEmpty(response.Items);
        await repository.DidNotReceive().GetBasketAsync(Arg.Any<string>());
    }

    [TestMethod]
    public async Task GetBasket_returns_an_empty_response_when_the_repository_has_no_basket()
    {
        var repository = Substitute.For<IBasketRepository>();
        repository.GetBasketAsync("alice").Returns((ModelCustomerBasket?)null);

        var response = await CreateService(repository).GetBasket(new GetBasketRequest(), CreateContext("alice"));

        Assert.IsEmpty(response.Items);
    }

    [TestMethod]
    public async Task GetBasket_returns_the_mapped_basket_when_present()
    {
        var repository = Substitute.For<IBasketRepository>();
        repository.GetBasketAsync("alice").Returns(new ModelCustomerBasket("alice")
        {
            Items = [new ModelBasketItem { ProductId = 7, Quantity = 3 }],
        });

        var response = await CreateService(repository).GetBasket(new GetBasketRequest(), CreateContext("alice"));

        Assert.HasCount(1, response.Items);
        Assert.AreEqual(7, response.Items[0].ProductId);
        Assert.AreEqual(3, response.Items[0].Quantity);
    }

    [TestMethod]
    public async Task UpdateBasket_throws_unauthenticated_when_there_is_no_user()
    {
        var repository = Substitute.For<IBasketRepository>();

        var ex = await Assert.ThrowsExactlyAsync<RpcException>(
            () => CreateService(repository).UpdateBasket(new UpdateBasketRequest(), CreateContext(null)));
        Assert.AreEqual(StatusCode.Unauthenticated, ex.StatusCode);
    }

    [TestMethod]
    public async Task UpdateBasket_throws_not_found_when_the_repository_returns_null()
    {
        var repository = Substitute.For<IBasketRepository>();
        repository.UpdateBasketAsync(Arg.Any<ModelCustomerBasket>()).Returns((ModelCustomerBasket?)null);

        var ex = await Assert.ThrowsExactlyAsync<RpcException>(
            () => CreateService(repository).UpdateBasket(new UpdateBasketRequest(), CreateContext("alice")));
        Assert.AreEqual(StatusCode.NotFound, ex.StatusCode);
    }

    [TestMethod]
    public async Task UpdateBasket_returns_the_mapped_basket_on_success()
    {
        var repository = Substitute.For<IBasketRepository>();
        repository.UpdateBasketAsync(Arg.Any<ModelCustomerBasket>()).Returns(new ModelCustomerBasket("alice")
        {
            Items = [new ModelBasketItem { ProductId = 7, Quantity = 3 }],
        });

        var request = new UpdateBasketRequest { Items = { new BasketItem { ProductId = 7, Quantity = 3 } } };
        var response = await CreateService(repository).UpdateBasket(request, CreateContext("alice"));

        Assert.HasCount(1, response.Items);
        await repository.Received(1).UpdateBasketAsync(Arg.Is<ModelCustomerBasket>(b => b.BuyerId == "alice"));
    }

    [TestMethod]
    public async Task DeleteBasket_throws_unauthenticated_when_there_is_no_user()
    {
        var repository = Substitute.For<IBasketRepository>();

        var ex = await Assert.ThrowsExactlyAsync<RpcException>(
            () => CreateService(repository).DeleteBasket(new DeleteBasketRequest(), CreateContext(null)));
        Assert.AreEqual(StatusCode.Unauthenticated, ex.StatusCode);
    }

    [TestMethod]
    public async Task DeleteBasket_deletes_the_callers_basket()
    {
        var repository = Substitute.For<IBasketRepository>();

        await CreateService(repository).DeleteBasket(new DeleteBasketRequest(), CreateContext("alice"));

        await repository.Received(1).DeleteBasketAsync("alice");
    }
}
