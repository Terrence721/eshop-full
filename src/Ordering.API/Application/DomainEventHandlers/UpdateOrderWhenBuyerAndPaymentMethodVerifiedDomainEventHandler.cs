namespace eShop.Ordering.API.Application.DomainEventHandlers
{
    /// <summary>
    /// Updates an order with the verified buyer and payment method IDs.
    /// </summary>
    public class UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler(
        IOrderRepository orderRepository,
        ILogger<UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler> logger)
        : INotificationHandler<BuyerAndPaymentMethodVerifiedDomainEvent>
    {
        private readonly IOrderRepository _orderRepository =
            orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        private readonly ILogger _logger =
            logger ?? throw new ArgumentNullException(nameof(logger));

        // Domain Logic comment:
        // When the Buyer and Buyer's payment method have been created or verified that they existed,
        // then we can update the original Order with the BuyerId and PaymentId (foreign keys)
        public async Task Handle(BuyerAndPaymentMethodVerifiedDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            // GetAsync is nullable by signature, but upstream has no guard here either --
            // preserving its exact assumption (the order that triggered this domain event
            // still exists) rather than adding a defensive check it doesn't have.
            var orderToUpdate = (await _orderRepository.GetAsync(domainEvent.OrderId))!;
            orderToUpdate.SetPaymentMethodVerified(domainEvent.Buyer.Id, domainEvent.Payment.Id);
            OrderingApiTrace.LogOrderPaymentMethodUpdated(_logger, domainEvent.OrderId, nameof(domainEvent.Payment), domainEvent.Payment.Id);
        }
    }
}
