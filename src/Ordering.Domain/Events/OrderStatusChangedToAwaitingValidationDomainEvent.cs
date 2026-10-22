namespace eShop.Ordering.Domain.Events
{
    /// <summary>
    /// Event used when the grace period order is confirmed
    /// </summary>
    public class OrderStatusChangedToAwaitingValidationDomainEvent(int orderId,
        IEnumerable<OrderItem> orderItems)
                    : INotification
    {
        public int OrderId { get; } = orderId;
        public IEnumerable<OrderItem> OrderItems { get; } = orderItems;
    }
}
