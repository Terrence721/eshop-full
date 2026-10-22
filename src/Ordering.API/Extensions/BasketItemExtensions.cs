namespace eShop.Ordering.API.Extensions
{
    /// <summary>
    /// Mapping extensions from the basket contract's <c>BasketItem</c> (Ordering.API's
    /// application model) to ordering DTOs used in order creation.
    /// </summary>
    public static class BasketItemExtensions
    {
        public static IEnumerable<OrderItemDTO> ToOrderItemsDTO(this IEnumerable<BasketItem> basketItems)
        {
            foreach (var item in basketItems)
            {
                yield return item.ToOrderItemDTO();
            }
        }

        public static OrderItemDTO ToOrderItemDTO(this BasketItem item)
        {
            return new OrderItemDTO()
            {
                ProductId = item.ProductId,
                // BasketItem.ProductName/PictureUrl are string? (no validator guarantees them --
                // see CreateOrderDraftCommand), but every real basket item carries real catalog
                // data, so null-forgiving here instead of retyping OrderItemDTO, which would
                // cascade into Order.AddOrderItem's non-nullable string parameters elsewhere.
                ProductName = item.ProductName!,
                PictureUrl = item.PictureUrl!,
                UnitPrice = item.UnitPrice,
                Units = item.Quantity
            };
        }
    }
}
