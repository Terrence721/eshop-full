namespace IdentityServerHost.Quickstart.UI;

public class ConsentViewModel : ConsentInputModel
{
    public required string ClientName { get; set; }
    public string? ClientUrl { get; set; }
    public string? ClientLogoUrl { get; set; }
    public bool AllowRememberConsent { get; set; }

    public required IEnumerable<ScopeViewModel> IdentityScopes { get; set; }
    public required IEnumerable<ScopeViewModel> ApiScopes { get; set; }
}
