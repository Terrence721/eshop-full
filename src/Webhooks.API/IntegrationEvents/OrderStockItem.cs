namespace Webhooks.API.IntegrationEvents
{
    /// <summary>
    /// Stock item line carried by order status change integration events.
    /// </summary>
    public record OrderStockItem(int ProductId, int Units);
}