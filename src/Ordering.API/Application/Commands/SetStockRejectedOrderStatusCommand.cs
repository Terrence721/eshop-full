namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// Command transitioning an order to stock-rejected status with rejected line items.
/// </summary>
public record SetStockRejectedOrderStatusCommand(int OrderNumber, List<int> OrderStockItems) : IRequest<bool>;
