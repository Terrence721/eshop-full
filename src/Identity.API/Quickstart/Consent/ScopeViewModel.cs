namespace IdentityServerHost.Quickstart.UI;

public class ScopeViewModel
{
    public required string Value { get; set; }
    public required string DisplayName { get; set; }
    public string? Description { get; set; }
    public bool Emphasize { get; set; }
    public bool Required { get; set; }
    public bool Checked { get; set; }
}
