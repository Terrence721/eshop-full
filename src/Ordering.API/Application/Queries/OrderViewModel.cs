namespace eShop.Ordering.API.Application.Queries;

// All four records below are only ever constructed via object initializers in
// OrderQueries.cs, which sets every property every time -- the implicit parameterless
// constructor these body-style records get otherwise never runs in practice.
public record Orderitem
{
    public string ProductName { get; init; } = null!;
    public int Units { get; init; }
    public double UnitPrice { get; init; }
    public string PictureUrl { get; init; } = null!;
}

public record Order
{
    public int OrderNumber { get; init; }
    public DateTime Date { get; init; }
    public string Status { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string Street { get; init; } = null!;
    public string City { get; init; } = null!;
    public string State { get; init; } = null!;
    public string Zipcode { get; init; } = null!;
    public string Country { get; init; } = null!;
    public List<Orderitem> OrderItems { get; set; } = null!;
    public decimal Total { get; set; }
}

public record OrderSummary
{
    public int OrderNumber { get; init; }
    public DateTime Date { get; init; }
    public string Status { get; init; } = null!;
    public double Total { get; init; }
}

public record CardType
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
}
