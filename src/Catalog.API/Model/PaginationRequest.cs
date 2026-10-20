using System.ComponentModel;

namespace eShop.Catalog.API.Model;

/// <summary>
/// Paging input for Catalog list endpoints. Defaults are 10 items per page, starting at page 0.
/// </summary>
public record PaginationRequest(
    [property: Description("Number of items to return in a single page of results")]
    [property: DefaultValue(10)]
    int PageSize = 10,

    [property: Description("The index of the page of results to return")]
    [property: DefaultValue(0)]
    int PageIndex = 0
);
