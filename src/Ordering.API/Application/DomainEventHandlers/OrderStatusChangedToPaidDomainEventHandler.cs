namespace eShop.Ordering.API.Application.DomainEventHandlers;

/// <summary>
/// Publishes an order-status-changed (paid) integration event and stock list.
/// </summary>
public class OrderStatusChangedToPaidDomainEventHandler(
    IOrderRepository orderRepository,
    ILogger<OrderStatusChangedToPaidDomainEventHandler> logger,
    IBuyerRepository buyerRepository,
    IOrderingIntegrationEventService orderingIntegrationEventService) : INotificationHandler<OrderStatusChangedToPaidDomainEvent>
{
    private readonly IOrderRepository _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IBuyerRepository _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentNullException(nameof(orderingIntegrationEventService));

    public async Task Handle(OrderStatusChangedToPaidDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        OrderingApiTrace.LogOrderStatusUpdated(_logger, domainEvent.OrderId, OrderStatus.Paid);

        // GetAsync/FindByIdAsync are nullable by signature, but upstream has no guard here
        // either -- preserving its exact assumption (the order/buyer that triggered this
        // domain event still exist) rather than adding defensive checks it doesn't have.
        var order = (await _orderRepository.GetAsync(domainEvent.OrderId))!;
        var buyer = (await _buyerRepository.FindByIdAsync(order.BuyerId!.Value))!;

        var orderStockList = domainEvent.OrderItems
            .Select(orderItem => new OrderStockItem(orderItem.ProductId, orderItem.Units));

        var integrationEvent = new OrderStatusChangedToPaidIntegrationEvent(
            domainEvent.OrderId,
            order.OrderStatus,
            buyer.Name,
            buyer.IdentityGuid,
            orderStockList);

        await _orderingIntegrationEventService.AddAndSaveEventAsync(integrationEvent);
    }
}
