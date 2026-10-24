namespace eShop.Catalog.API.Infrastructure
{
    /// <summary>
    /// EF Core context for the Catalog aggregate, including the pgvector extension
    /// and the transactional outbox table used for integration events.
    /// </summary>
    /// <remarks>
    /// Add migrations using the following command inside the 'Catalog.API' project directory:
    ///
    /// dotnet ef migrations add --context CatalogContext [migration-name]
    /// </remarks>
    public class CatalogContext(DbContextOptions<CatalogContext> options) : DbContext(options)
    {
        public DbSet<CatalogItem> CatalogItems { get; set; } = null!;
        public DbSet<CatalogBrand> CatalogBrands { get; set; } = null!;
        public DbSet<CatalogType> CatalogTypes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresExtension("vector");
            builder.ApplyConfiguration(new CatalogBrandEntityTypeConfiguration());
            builder.ApplyConfiguration(new CatalogTypeEntityTypeConfiguration());
            builder.ApplyConfiguration(new CatalogItemEntityTypeConfiguration());

            // Add the outbox table to this context
            builder.UseIntegrationEventLogs();
        }
    }
}
