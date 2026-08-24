namespace IdentityServerHost.Quickstart.UI;

public class DiagnosticsViewModel
{
    public required IEnumerable<ClaimViewModel> Claims { get; set; }
    public required IDictionary<string, string?> Properties { get; set; }
    public required IEnumerable<string> Clients { get; set; }
}

public class ClaimViewModel
{
    public required string Type { get; set; }
    public required string Value { get; set; }
}
