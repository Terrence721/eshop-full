namespace IdentityServerHost.Quickstart.UI;

// Shared by ConsentInputModel and DeviceAuthorizationInputModel - the fields
// both POST bodies actually carry to describe the user's consent decision.
// Deliberately NOT ReturnUrl: that's Consent-only (used to redirect back to
// the authorization endpoint in a browser); the device flow has no browser
// redirect to make, so DeviceAuthorizationInputModel inheriting it was dead
// data, same class of issue ConsentScopesViewModel's split already fixed on
// the response side.
public class ConsentDecisionInputModel
{
    public string? Button { get; set; }
    public IEnumerable<string>? ScopesConsented { get; set; }
    public bool RememberConsent { get; set; }
    public string? Description { get; set; }
}
