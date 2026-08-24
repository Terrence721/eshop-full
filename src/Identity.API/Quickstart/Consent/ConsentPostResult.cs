namespace IdentityServerHost.Quickstart.UI;

// Response shape for POST /Consent/Index - the outcome is one of a redirect, a
// validation error (with the form redisplayed), or the redisplayed form alone.
// A separate type from ConsentViewModel (used for GET) since ConsentViewModel's
// ClientName/IdentityScopes/ApiScopes are required and meaningless for a pure
// redirect outcome.
public class ConsentPostResult
{
    public string? RedirectUrl { get; set; }
    public bool IsNativeClient { get; set; }
    public string? ValidationError { get; set; }
    public ConsentViewModel? ViewModel { get; set; }
}
