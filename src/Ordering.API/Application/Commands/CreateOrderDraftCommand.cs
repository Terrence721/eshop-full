namespace eShop.Ordering.API.Application.Commands;

using eShop.Ordering.API.Application.Models;

/// <summary>
/// Command requesting a read-only order draft for a buyer and basket items.
/// </summary>
public record CreateOrderDraftCommand(string BuyerId, IEnumerable<BasketItem> Items) : IRequest<OrderDraftDTO>;