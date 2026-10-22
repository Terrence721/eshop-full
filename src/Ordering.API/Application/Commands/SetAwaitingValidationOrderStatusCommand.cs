namespace eShop.Ordering.API.Application.Commands
{
    /// <summary>
    /// Command transitioning an order to the awaiting-validation status.
    /// </summary>
    public record SetAwaitingValidationOrderStatusCommand(int OrderNumber) : IRequest<bool>;
}
