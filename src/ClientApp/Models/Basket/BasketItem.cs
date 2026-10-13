namespace eShop.ClientApp.Models.Basket;

public class BasketItem : BindableObject
{
    private int _quantity;

    public string Id { get; set; } = null!;

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public decimal OldUnitPrice { get; set; }

    public bool HasNewPrice => OldUnitPrice != 0.0m;

    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            OnPropertyChanged();
        }
    }

    public string PictureUrl { get; set; } = null!;

    public decimal Total => Quantity * UnitPrice;

    public override string ToString()
    {
        return $"Product Id: {ProductId}, Quantity: {Quantity}";
    }
}
