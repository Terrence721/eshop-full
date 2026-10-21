namespace eShop.Ordering.API.Application.Commands;

using eShop.Ordering.API.Extensions;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

// Regular CommandHandler
/// <summary>
/// Builds a read-only order draft from a buyer basket.
/// </summary>
public class CreateOrderDraftCommandHandler
    : IRequestHandler<CreateOrderDraftCommand, OrderDraftDTO>
{
    public Task<OrderDraftDTO> Handle(CreateOrderDraftCommand message, CancellationToken cancellationToken)
    {
        var order = Order.NewDraft();
        var orderItems = message.Items.Select(i => i.ToOrderItemDTO());
        foreach (var item in orderItems)
        {
            order.AddOrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        return Task.FromResult(OrderDraftDTO.FromOrder(order));
    }
}

public record OrderDraftDTO
{
    // Only ever constructed via FromOrder() below, which always sets this -- the record's
    // implicit parameterless constructor otherwise leaves it unassigned.
    public IEnumerable<OrderItemDTO> OrderItems { get; init; } = null!;
    public decimal Total { get; init; }

    public static OrderDraftDTO FromOrder(Order order)
    {
        return new OrderDraftDTO()
        {
            OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
            {
                Discount = oi.Discount,
                ProductId = oi.ProductId,
                UnitPrice = oi.UnitPrice,
                PictureUrl = oi.PictureUrl,
                Units = oi.Units,
                ProductName = oi.ProductName
            }),
            Total = order.GetTotal()
        };
    }
}

public record OrderItemDTO
{
    public int ProductId { get; init; }

    // Always set via the two call sites that construct this (FromOrder() above and
    // BasketItemExtensions.ToOrderItemDTO()) -- never via the implicit parameterless ctor.
    public string ProductName { get; init; } = null!;

    public decimal UnitPrice { get; init; }

    public decimal Discount { get; init; }

    public int Units { get; init; }

    public string PictureUrl { get; init; } = null!;
}