using System.Text.Json.Serialization;

namespace eShop.ClientApp.Models.Orders
{
    public class OrderItem
    {
        // The real Ordering.API order response has no per-item ProductId/OrderId
        // (see Ordering.API.Application.Queries.Orderitem) - these stay unset in real mode.
        public long ProductId { get; set; }

        public Guid? OrderId { get; set; }

        [JsonPropertyName("unitprice")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("productname")]
        public string ProductName { get; set; } = null!;

        [JsonPropertyName("pictureurl")]
        public string PictureUrl { get; set; } = null!;

        [JsonPropertyName("units")]
        public int Quantity { get; set; }

        public decimal Discount { get; set; }
        public decimal Total => Quantity * UnitPrice;

        public override string ToString()
        {
            return string.Format("Product Id: {0}, Quantity: {1}", ProductId, Quantity);
        }
    }
}
