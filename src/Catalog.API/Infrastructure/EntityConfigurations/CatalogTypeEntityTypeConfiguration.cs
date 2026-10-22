namespace eShop.Catalog.API.Infrastructure.EntityConfigurations
{
    /// <summary>
    /// EF Core mapping for <see cref="CatalogType"/>, including its table name and column constraints.
    /// </summary>
    class CatalogTypeEntityTypeConfiguration
        : IEntityTypeConfiguration<CatalogType>
    {
        public void Configure(EntityTypeBuilder<CatalogType> builder)
        {
            builder.ToTable("CatalogType");

            builder.Property(cb => cb.Type)
                .HasMaxLength(100);
        }
    }
}
