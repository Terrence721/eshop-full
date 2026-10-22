namespace eShop.Ordering.API.Application.Models;

/// <summary>
/// Application-model BasketItem synced from Catalog (used for draft/creation).
/// </summary>
public class BasketItem
{
    public string? Id { get; init; }
    public int ProductId { get; init; }
    public string? ProductName { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal OldUnitPrice { get; init; }
    public int Quantity { get; init; }
    public string? PictureUrl { get; init; }
}
