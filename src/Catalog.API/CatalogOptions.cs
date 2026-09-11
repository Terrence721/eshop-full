namespace eShop.Catalog.API;

public class CatalogOptions
{
    public string? PicBaseUrl { get; set; }
    public bool UseCustomizationData { get; set; }

    // Not from upstream: upstream hardcodes this as a private const in CatalogAI,
    // truncating whatever the configured embedding model produces down to this
    // many dimensions -- only safe for models specifically trained for
    // Matryoshka-style truncation. Made configurable rather than left hardcoded,
    // since the right value depends on whichever embedding model this fork ends
    // up shipping with -- a decision still open.
    public int EmbeddingDimensions { get; set; } = 384;
}
