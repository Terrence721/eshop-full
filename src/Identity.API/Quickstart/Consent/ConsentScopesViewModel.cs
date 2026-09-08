namespace IdentityServerHost.Quickstart.UI;

// Shared by ConsentViewModel and DeviceAuthorizationViewModel - the fields both
// GET-response shapes actually need to redisplay a scope-consent screen.
// Deliberately NOT ConsentInputModel: that type is POST-only (Button has no
// meaning on a GET response), and Button was leaking into every GET response
// before this split existed.
public class ConsentScopesViewModel
{
    public bool RememberConsent { get; set; }
    public IEnumerable<string>? ScopesConsented { get; set; }
    public string? Description { get; set; }

    public required string ClientName { get; set; }
    public string? ClientUrl { get; set; }
    public string? ClientLogoUrl { get; set; }
    public bool AllowRememberConsent { get; set; }

    public required IEnumerable<ScopeViewModel> IdentityScopes { get; set; }
    public required IEnumerable<ScopeViewModel> ApiScopes { get; set; }
}
