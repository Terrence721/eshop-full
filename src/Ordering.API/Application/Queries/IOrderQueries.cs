namespace eShop.Ordering.API.Application.Queries
{
    /// <summary>
    /// Ordering read-model queries (order details, user orders, cards).
    /// </summary>
    public interface IOrderQueries
    {
        Task<Order> GetOrderAsync(int id);

        Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(string userId);

        Task<IEnumerable<CardType>> GetCardTypesAsync();
    }
}
