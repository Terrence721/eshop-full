namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// Command transitioning an order to the paid status.
/// </summary>
public record SetPaidOrderStatusCommand(int OrderNumber) : IRequest<bool>;
