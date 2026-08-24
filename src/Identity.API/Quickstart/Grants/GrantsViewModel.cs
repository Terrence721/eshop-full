namespace IdentityServerHost.Quickstart.UI;

public class GrantsViewModel
{
    public required IEnumerable<GrantViewModel> Grants { get; set; }
}

public class GrantViewModel
{
    public required string ClientId { get; set; }
    public required string ClientName { get; set; }
    public string? ClientUrl { get; set; }
    public string? ClientLogoUrl { get; set; }
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Expires { get; set; }
    public required IEnumerable<string> IdentityGrantNames { get; set; }
    public required IEnumerable<string> ApiGrantNames { get; set; }
}
