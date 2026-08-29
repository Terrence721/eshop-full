namespace IdentityServerHost.Quickstart.UI;

// Response shape for POST /Account/Login - the outcome is one of a redirect
// (cancelled or succeeded) or the redisplayed form with a validation error.
// A separate type from LoginViewModel, matching ConsentPostResult's reasoning:
// a pure redirect outcome has no login-page state to describe.
public class LoginPostResult
{
    public string? RedirectUrl { get; set; }
    public bool IsNativeClient { get; set; }
    public LoginViewModel? ViewModel { get; set; }
    public string? ValidationError { get; set; }
}
