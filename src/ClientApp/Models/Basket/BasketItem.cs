using CommunityToolkit.Mvvm.ComponentModel;

namespace eShop.ClientApp.Models.Basket;

public partial class BasketItem : ObservableObject
{
    public string Id { get; set; } = null!;

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public decimal OldUnitPrice { get; set; }

    public bool HasNewPrice => OldUnitPrice != 0.0m;

    [ObservableProperty]
    public partial int Quantity { get; set; }

    public string PictureUrl { get; set; } = null!;

    public decimal Total => Quantity * UnitPrice;

    public override string ToString()
    {
        return $"Product Id: {ProductId}, Quantity: {Quantity}";
    }
}
