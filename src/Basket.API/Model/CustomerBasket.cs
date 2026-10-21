namespace eShop.Basket.API.Model;

/// <summary>
/// A customer's basket, containing line items.
/// </summary>
public class CustomerBasket
{
    public string? BuyerId { get; set; }

    public List<BasketItem> Items { get; set; } = [];

    public CustomerBasket() { }

    public CustomerBasket(string customerId)
    {
        BuyerId = customerId;
    }
}
