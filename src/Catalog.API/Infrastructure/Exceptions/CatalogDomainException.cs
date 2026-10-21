namespace eShop.Catalog.API.Infrastructure.Exceptions;

/// <summary>
/// Exception type for domain (catalog) errors
/// </summary>
public class CatalogDomainException : Exception
{
    public CatalogDomainException()
    { }

    public CatalogDomainException(string message)
        : base(message)
    { }

    public CatalogDomainException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
