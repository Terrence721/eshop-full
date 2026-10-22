using System.ComponentModel.DataAnnotations;
using eShop.ClientApp.Models.Orders;

namespace eShop.ClientApp.Models.Basket
{
    public class OrderCheckout
    {
        [Required]
        public string City { get; set; } = null!;
        [Required]
        public string Street { get; set; } = null!;
        [Required]
        public string State { get; set; } = null!;
        [Required]
        public string Country { get; set; } = null!;

        public string ZipCode { get; set; } = null!;
        [Required]
        public string CardNumber { get; set; } = null!;
        [Required]
        public string CardHolderName { get; set; } = null!;

        [Required]
        public DateTime CardExpiration { get; set; }

        [Required]
        public string CardSecurityNumber { get; set; } = null!;

        public int CardTypeId { get; set; }

        public string Buyer { get; set; } = null!;

        public IList<OrderItem> Items { get; set; } = null!;

        [Required]
        public Guid RequestId { get; set; }
    }
}
