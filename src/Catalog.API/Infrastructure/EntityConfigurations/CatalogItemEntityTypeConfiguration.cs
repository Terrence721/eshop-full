namespace eShop.Catalog.API.Infrastructure.EntityConfigurations
{
    /// <summary>
    /// EF Core mapping for <see cref="CatalogItem"/>, including column lengths,
    /// the pgvector embedding type, foreign keys and the Name index.
    /// </summary>
    class CatalogItemEntityTypeConfiguration
        : IEntityTypeConfiguration<CatalogItem>
    {
        public void Configure(EntityTypeBuilder<CatalogItem> builder)
        {
            builder.ToTable("Catalog");

            builder.Property(ci => ci.Name)
                .HasMaxLength(50);

            builder.Property(ci => ci.Embedding)
                .HasColumnType("vector(384)");

            builder.HasOne(ci => ci.CatalogBrand)
                .WithMany();

            builder.HasOne(ci => ci.CatalogType)
                .WithMany();

            builder.HasIndex(ci => ci.Name);
        }
    }
}
