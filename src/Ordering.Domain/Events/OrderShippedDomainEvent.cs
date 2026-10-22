namespace eShop.Ordering.Domain.Events;

public class OrderShippedDomainEvent(Order order) : INotification
{
    public Order Order { get; } = order;
}
