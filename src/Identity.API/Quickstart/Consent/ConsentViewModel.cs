namespace IdentityServerHost.Quickstart.UI;

public class ConsentViewModel : ConsentInputModel
{
    public required string ClientName { get; set; }
    public string? ClientUrl { get; set; }
    public string? ClientLogoUrl { get; set; }
    public bool AllowRememberConsent { get; set; }

    public required IEnumerable<ScopeViewModel> IdentityScopes { get; set; }
    public required IEnumerable<ScopeViewModel> ApiScopes { get; set; }

    // Only populated on the POST /Consent/Index response, when the outcome is a redirect
    // rather than redisplaying the form - see ConsentController.
    public string? RedirectUrl { get; set; }
    public bool IsNativeClient { get; set; }
    public string? ValidationError { get; set; }
}
