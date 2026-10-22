namespace eShop.Ordering.API.Application.Commands
{
    /// <summary>
    /// Command requesting shipment of an order.
    /// </summary>
    public record ShipOrderCommand(int OrderNumber) : IRequest<bool>;
}
