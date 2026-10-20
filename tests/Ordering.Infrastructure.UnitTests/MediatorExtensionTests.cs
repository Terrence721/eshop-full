using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Domain.Events;
using MediatR;
using NSubstitute;

namespace eShop.Ordering.Infrastructure.UnitTests;

[TestClass]
public class MediatorExtensionTests
{
    private static Buyer CreateBuyerWithARaisedDomainEvent()
    {
        var buyer = new Buyer("identity-1", "Alice");
        buyer.VerifyOrAddPaymentMethod(
            1, "alias", "4111111111111111", "123", "Alice", DateTime.UtcNow.AddYears(1), orderId: 1);
        return buyer;
    }

    [TestMethod]
    public async Task DispatchDomainEventsAsync_publishes_every_tracked_entitys_domain_events()
    {
        var mediator = Substitute.For<IMediator>();
        var context = TestOrderingContext.Create(mediator);
        var buyer = CreateBuyerWithARaisedDomainEvent();
        context.Add(buyer);

        await mediator.DispatchDomainEventsAsync(context);

        await mediator.Received(1).Publish(Arg.Any<BuyerAndPaymentMethodVerifiedDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task DispatchDomainEventsAsync_clears_domain_events_after_dispatching()
    {
        var mediator = Substitute.For<IMediator>();
        var context = TestOrderingContext.Create(mediator);
        var buyer = CreateBuyerWithARaisedDomainEvent();
        context.Add(buyer);

        await mediator.DispatchDomainEventsAsync(context);

        Assert.IsEmpty(buyer.DomainEvents!);
    }

    [TestMethod]
    public async Task DispatchDomainEventsAsync_does_nothing_when_no_tracked_entity_has_domain_events()
    {
        var mediator = Substitute.For<IMediator>();
        var context = TestOrderingContext.Create(mediator);
        context.Add(new Buyer("identity-1", "Alice"));

        await mediator.DispatchDomainEventsAsync(context);

        await mediator.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
