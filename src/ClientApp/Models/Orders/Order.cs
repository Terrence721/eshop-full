using System.Text.Json.Serialization;

namespace eShop.ClientApp.Models.Orders;

public class Order
{
    public Order()
    {
        SequenceNumber = 1;
        OrderItems = [];
    }

    // Not present on the real Ordering.API order response (Application.Queries.Order has
    // no UserId/UserName) - these stay unset in real mode.
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    public int SequenceNumber { get; set; }

    [JsonPropertyName("date")] public DateTime OrderDate { get; set; }

    [JsonPropertyName("status")] public string OrderStatus { get; set; } = null!;

    [JsonPropertyName("city")] public string ShippingCity { get; set; } = null!;

    [JsonPropertyName("street")] public string ShippingStreet { get; set; } = null!;

    [JsonPropertyName("state")] public string ShippingState { get; set; } = null!;

    [JsonPropertyName("country")] public string ShippingCountry { get; set; } = null!;

    [JsonPropertyName("zipCode")] public string ShippingZipCode { get; set; } = null!;

    public int CardTypeId { get; set; }

    public string? CardNumber { get; set; }

    public string? CardHolderName { get; set; }

    public DateTime CardExpiration { get; set; }

    public string? CardSecurityNumber { get; set; }

    [JsonPropertyName("orderItems")]
    public List<OrderItem> OrderItems { get; set; }

    [JsonPropertyName("total")] public decimal Total { get; set; }

    [JsonPropertyName("ordernumber")] public int OrderNumber { get; set; }
}
