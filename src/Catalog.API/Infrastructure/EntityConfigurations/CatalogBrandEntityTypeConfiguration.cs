namespace eShop.Catalog.API.Infrastructure.EntityConfigurations;

/// <summary>
/// EF Core mapping for <see cref="CatalogBrand"/>, including its table name and column constraints.
/// </summary>
class CatalogBrandEntityTypeConfiguration
    : IEntityTypeConfiguration<CatalogBrand>
{
    public void Configure(EntityTypeBuilder<CatalogBrand> builder)
    {
        builder.ToTable("CatalogBrand");

        builder.Property(cb => cb.Brand)
            .HasMaxLength(100);
    }
}
