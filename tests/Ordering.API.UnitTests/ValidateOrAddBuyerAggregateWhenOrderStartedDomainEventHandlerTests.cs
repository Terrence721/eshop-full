using eShop.Ordering.API.Application.DomainEventHandlers;
using eShop.Ordering.API.Application.IntegrationEvents;
using eShop.Ordering.API.Application.IntegrationEvents.Events;
using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Domain.Events;
using eShop.Ordering.Domain.SeedWork;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandlerTests
{
    private static OrderStartedDomainEvent CreateEvent() =>
        new(new Order("user-1", "Alice", new Address("123 Main St", "Springfield", "IL", "USA", "62704"),
                1, "4111111111111111", "123", "Alice", DateTime.UtcNow.AddYears(1)),
            "user-1", "Alice", 1, "4111111111111111", "123", "Alice", DateTime.UtcNow.AddYears(1));

    private static ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler CreateHandler(
        IBuyerRepository buyerRepository, IOrderingIntegrationEventService integrationEventService)
    {
        return new ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler(
            Substitute.For<ILogger<ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler>>(),
            buyerRepository,
            integrationEventService);
    }

    [TestMethod]
    public async Task Handle_adds_a_new_buyer_when_one_does_not_already_exist()
    {
        var buyerRepository = Substitute.For<IBuyerRepository>();
        buyerRepository.FindAsync(Arg.Any<string>()).Returns((Buyer?)null);
        buyerRepository.UnitOfWork.Returns(Substitute.For<IUnitOfWork>());
        var integrationEventService = Substitute.For<IOrderingIntegrationEventService>();
        var handler = CreateHandler(buyerRepository, integrationEventService);

        await handler.Handle(CreateEvent(), CancellationToken.None);

        buyerRepository.Received(1).Add(Arg.Is<Buyer>(b => b.IdentityGuid == "user-1" && b.Name == "Alice"));
        await integrationEventService.Received(1).AddAndSaveEventAsync(
            Arg.Is<OrderStatusChangedToSubmittedIntegrationEvent>(e => e.BuyerIdentityGuid == "user-1" && e.BuyerName == "Alice"));
    }

    [TestMethod]
    public async Task Handle_does_not_add_a_buyer_that_already_exists()
    {
        var existingBuyer = new Buyer("user-1", "Alice From The Buyer Record");
        var buyerRepository = Substitute.For<IBuyerRepository>();
        buyerRepository.FindAsync(Arg.Any<string>()).Returns(existingBuyer);
        buyerRepository.UnitOfWork.Returns(Substitute.For<IUnitOfWork>());
        var integrationEventService = Substitute.For<IOrderingIntegrationEventService>();
        var handler = CreateHandler(buyerRepository, integrationEventService);

        await handler.Handle(CreateEvent(), CancellationToken.None);

        buyerRepository.DidNotReceive().Add(Arg.Any<Buyer>());
        await integrationEventService.Received(1).AddAndSaveEventAsync(
            Arg.Is<OrderStatusChangedToSubmittedIntegrationEvent>(e => e.BuyerName == "Alice From The Buyer Record"));
    }
}
