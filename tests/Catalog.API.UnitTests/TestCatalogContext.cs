using eShop.Catalog.API.Infrastructure;
using eShop.Catalog.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace eShop.Catalog.API.UnitTests;

// CatalogItem.Embedding is a Pgvector.Vector -- a Postgres-only type the
// InMemory provider has no mapping for at all, so CatalogContext's own
// OnModelCreating fails model validation unmodified. This customizer runs
// after it and ignores just that one property for the test model, the
// standard EF Core pattern for this exact situation (not something
// reachable by configuring the InMemory provider itself).
file sealed class IgnoreEmbeddingModelCustomizer(ModelCustomizerDependencies dependencies) : ModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        modelBuilder.Entity<CatalogItem>().Ignore(item => item.Embedding);
    }
}

// Shared by every test that needs a real CatalogContext for DbSet queries
// (Find, SaveChangesAsync) without a Postgres instance -- a fresh InMemory
// database per call, named uniquely so tests don't see each other's data.
internal static class TestCatalogContext
{
    public static CatalogContext Create() => CreateNamed(Guid.NewGuid().ToString());

    // For tests that need to verify a save actually persisted (not just that
    // the tracked instance mutated in place) -- open a second context on the
    // same database name after the first is done with it.
    public static CatalogContext CreateNamed(string dbName)
    {
        var options = new DbContextOptionsBuilder<CatalogContext>()
            .UseInMemoryDatabase(dbName)
            .ReplaceService<IModelCustomizer, IgnoreEmbeddingModelCustomizer>()
            .Options;

        // The `required` DbSet properties force an object initializer on a
        // `new CatalogContext(options)` expression -- but EF Core already
        // auto-wires those DbSets inside the base constructor, so anything
        // assigned afterward (even non-null placeholders) clobbers EF's own
        // real instances. required-member enforcement is a C#-source-level
        // check on `new` expressions, not a runtime constraint, so
        // constructing via reflection sidesteps it while still running the
        // same real constructor (and EF's real auto-init) underneath.
        return (CatalogContext)Activator.CreateInstance(typeof(CatalogContext), options)!;
    }
}
