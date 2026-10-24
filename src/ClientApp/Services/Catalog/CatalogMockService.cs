using System.Text.Json;
using Avalonia.Platform;
using eShop.ClientApp.Models.Catalog;

namespace eShop.ClientApp.Services.Catalog
{
    public class CatalogMockService : ICatalogService
    {
        private sealed class CatalogSourceEntry
        {
            public int Id { get; set; }
            public string? Type { get; set; }
            public string? Brand { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public decimal Price { get; set; }
        }

        private static readonly List<CatalogBrand> MockCatalogBrands;
        private static readonly List<CatalogType> MockCatalogTypes;
        private static readonly List<CatalogItem> MockCatalog;

        static CatalogMockService()
        {
            using var stream = AssetLoader.Open(new Uri("avares://ClientApp/Assets/catalog.json"));
            var entries = JsonSerializer.Deserialize<List<CatalogSourceEntry>>(stream) ?? [];

            MockCatalogBrands = entries
                .Where(e => e.Brand is not null)
                .Select(e => e.Brand!)
                .Distinct()
                .Select((b, i) => new CatalogBrand { Id = i + 1, Brand = b })
                .ToList();

            MockCatalogTypes = entries
                .Where(e => e.Type is not null)
                .Select(e => e.Type!)
                .Distinct()
                .Select((t, i) => new CatalogType { Id = i + 1, Type = t })
                .ToList();

            var brandIds = MockCatalogBrands.ToDictionary(b => b.Brand, b => b.Id);
            var typeIds = MockCatalogTypes.ToDictionary(t => t.Type, t => t.Id);

            MockCatalog = entries
                .Where(e => e.Name is not null && e.Brand is not null && e.Type is not null)
                .Select(e => new CatalogItem
                {
                    Id = e.Id,
                    Name = e.Name!,
                    Description = e.Description,
                    Price = e.Price,
                    CatalogBrandId = brandIds[e.Brand!],
                    CatalogBrand = MockCatalogBrands.First(b => b.Brand == e.Brand),
                    CatalogTypeId = typeIds[e.Type!],
                    CatalogType = MockCatalogTypes.First(t => t.Type == e.Type),
                    PictureUri = $"avares://ClientApp/Assets/Pics/{e.Id}.webp",
                })
                .ToList();
        }

        public Task<IEnumerable<CatalogItem>> FilterAsync(int catalogBrandId, int catalogTypeId)
            => Task.FromResult<IEnumerable<CatalogItem>>(MockCatalog
                .Where(i => i.CatalogBrandId == catalogBrandId && i.CatalogTypeId == catalogTypeId));

        public Task<IEnumerable<CatalogItem>> GetCatalogAsync()
            => Task.FromResult<IEnumerable<CatalogItem>>(MockCatalog);

        public Task<CatalogItem?> GetCatalogItemAsync(int catalogItemId)
            => Task.FromResult(MockCatalog.FirstOrDefault(i => i.Id == catalogItemId));

        public Task<IEnumerable<CatalogBrand>> GetCatalogBrandAsync()
            => Task.FromResult<IEnumerable<CatalogBrand>>(MockCatalogBrands);

        public Task<IEnumerable<CatalogType>> GetCatalogTypeAsync()
            => Task.FromResult<IEnumerable<CatalogType>>(MockCatalogTypes);
    }
}
