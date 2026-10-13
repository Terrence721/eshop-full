using eShop.Basket.API.IntegrationEvents.EventHandling;
using eShop.Basket.API.IntegrationEvents.Events;
using eShop.Basket.API.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Basket.API.UnitTests;

[TestClass]
public class OrderStartedIntegrationEventHandlerTests
{
    [TestMethod]
    public async Task Handle_deletes_the_basket_belonging_to_the_order_s_user()
    {
        var repository = Substitute.For<IBasketRepository>();
        var handler = new OrderStartedIntegrationEventHandler(repository, Substitute.For<ILogger<OrderStartedIntegrationEventHandler>>());

        await handler.Handle(new OrderStartedIntegrationEvent("alice"));

        await repository.Received(1).DeleteBasketAsync("alice");
    }
}
