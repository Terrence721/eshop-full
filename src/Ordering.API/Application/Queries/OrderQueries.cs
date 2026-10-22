namespace eShop.Ordering.API.Application.Queries
{
    /// <summary>
    /// EF Core read-model query implementation of IOrderQueries.
    /// </summary>
    public class OrderQueries(OrderingContext context)
        : IOrderQueries
    {
        public async Task<Order> GetOrderAsync(int id)
        {
            var order = await context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null)
                throw new KeyNotFoundException();

            return new Order
            {
                OrderNumber = order.Id,
                Date = order.OrderDate,
                Description = order.Description,
                City = order.Address.City,
                Country = order.Address.Country,
                State = order.Address.State,
                Street = order.Address.Street,
                Zipcode = order.Address.ZipCode,
                Status = order.OrderStatus.ToString(),
                Total = order.GetTotal(),
                OrderItems = order.OrderItems.Select(oi => new Orderitem
                {
                    ProductName = oi.ProductName,
                    Units = oi.Units,
                    UnitPrice = (double)oi.UnitPrice,
                    PictureUrl = oi.PictureUrl
                }).ToList()
            };
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(string userId)
        {
            return await context.Orders
                // Buyer is a genuinely nullable navigation property (new orders have no
                // BuyerId until linked), but this LINQ expression is translated to SQL --
                // EF Core's generated query handles the null case there, it never executes
                // as an in-memory null dereference.
                .Where(o => o.Buyer!.IdentityGuid == userId)
                .Select(o => new OrderSummary
                {
                    OrderNumber = o.Id,
                    Date = o.OrderDate,
                    Status = o.OrderStatus.ToString(),
                    Total = (double)o.OrderItems.Sum(oi => oi.UnitPrice * oi.Units)
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CardType>> GetCardTypesAsync() =>
            await context.CardTypes.Select(c => new CardType { Id = c.Id, Name = c.Name }).ToListAsync();
    }
}
