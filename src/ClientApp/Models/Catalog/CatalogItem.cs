namespace eShop.ClientApp.Models.Catalog
{
    public class CatalogItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
        public decimal Price { get; set; }

        // Populated by MockCatalogService's seed data; the real Catalog.API has no PictureUri
        // field (see WebApp's catalogItemPictureUrl) - the real CatalogService must compute this.
        public string? PictureUri { get; set; }
        public int CatalogBrandId { get; set; }
        public CatalogBrand? CatalogBrand { get; set; }
        public int CatalogTypeId { get; set; }
        public CatalogType? CatalogType { get; set; }
    }
}
