namespace eShop.Ordering.Domain.Events;

public class OrderCancelledDomainEvent(Order order) : INotification
{
    public Order Order { get; } = order;
}

