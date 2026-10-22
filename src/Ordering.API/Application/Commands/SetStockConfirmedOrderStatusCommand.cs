namespace eShop.Ordering.API.Application.Commands
{
    /// <summary>
    /// Command transitioning an order to stock-confirmed status.
    /// </summary>
    public record SetStockConfirmedOrderStatusCommand(int OrderNumber) : IRequest<bool>;
}
