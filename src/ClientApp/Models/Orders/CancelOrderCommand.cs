namespace eShop.ClientApp.Models.Orders;

public class CancelOrderCommand(int orderNumber)
{
    public int OrderNumber { get; } = orderNumber;
}
