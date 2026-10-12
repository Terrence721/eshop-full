using eShop.Catalog.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace eShop.Catalog.API.UnitTests;

// Shared by every test that needs a real CatalogContext for DbSet queries
// (Find, SaveChangesAsync) without a Postgres instance -- a fresh InMemory
// database per call, named uniquely so tests don't see each other's data.
internal static class TestCatalogContext
{
    public static CatalogContext Create()
    {
        var options = new DbContextOptionsBuilder<CatalogContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CatalogContext(options)
        {
            CatalogItems = null!,
            CatalogBrands = null!,
            CatalogTypes = null!,
        };
    }
}
