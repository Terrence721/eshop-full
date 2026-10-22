namespace eShop.Ordering.Domain.Events;

/// <summary>
/// Event used when the order is paid
/// </summary>
public class OrderStatusChangedToPaidDomainEvent(int orderId,
    IEnumerable<OrderItem> orderItems)
        : INotification
{
    public int OrderId { get; } = orderId;
    public IEnumerable<OrderItem> OrderItems { get; } = orderItems;
}
