using System.ComponentModel.DataAnnotations;

namespace eShop.Catalog.API.Model
{
    public class CatalogBrand(string brand)
    {
        public int Id { get; set; }

        [Required]
        public string Brand { get; set; } = brand;
    }
}
