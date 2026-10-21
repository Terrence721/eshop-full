namespace eShop.Ordering.API.Application.DomainEventHandlers;

/// <summary>
/// Validates or creates the buyer aggregate and payment method when an order starts.
/// </summary>
public class ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler(
    ILogger<ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler> logger,
    IBuyerRepository buyerRepository,
    IOrderingIntegrationEventService orderingIntegrationEventService)
    : INotificationHandler<OrderStartedDomainEvent>
{
    private readonly ILogger _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IBuyerRepository _buyerRepository =
        buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService =
        orderingIntegrationEventService ?? throw new ArgumentNullException(nameof(orderingIntegrationEventService));

    public async Task Handle(OrderStartedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var cardTypeId = domainEvent.CardTypeId != 0 ? domainEvent.CardTypeId : 1;
        var buyer = await _buyerRepository.FindAsync(domainEvent.UserId);
        var buyerExisted = buyer is not null;

        if (!buyerExisted)
        {
            buyer = new Buyer(domainEvent.UserId, domainEvent.UserName);
        }

        // REVIEW: The event this creates needs to be sent after SaveChanges has propagated the buyer Id. It currently only
        // works by coincidence. If we remove HiLo or if anything decides to yield earlier, it will break.

        // buyerExisted (set above from the same null-check) guarantees buyer is non-null
        // here on every path -- the compiler just can't correlate the two variables.
        buyer!.VerifyOrAddPaymentMethod(cardTypeId,
                                        $"Payment Method on {DateTime.UtcNow}",
                                        domainEvent.CardNumber,
                                        domainEvent.CardSecurityNumber,
                                        domainEvent.CardHolderName,
                                        domainEvent.CardExpiration,
                                        domainEvent.Order.Id);

        if (!buyerExisted)
        {
            _buyerRepository.Add(buyer);
        }

        await _buyerRepository.UnitOfWork
            .SaveEntitiesAsync(cancellationToken);

        var integrationEvent = new OrderStatusChangedToSubmittedIntegrationEvent(domainEvent.Order.Id, domainEvent.Order.OrderStatus, buyer.Name, buyer.IdentityGuid);
        await _orderingIntegrationEventService.AddAndSaveEventAsync(integrationEvent);
        OrderingApiTrace.LogOrderBuyerAndPaymentValidatedOrUpdated(_logger, buyer.Id, domainEvent.Order.Id);
    }
}
