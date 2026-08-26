namespace IdentityServerHost.Quickstart.UI;

public class AccountOptions
{
    public const bool AllowLocalLogin = true;
    public const bool AllowRememberLogin = true;
    public static readonly TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

    public static readonly bool ShowLogoutPrompt = false;
    public const bool AutomaticRedirectAfterSignOut = true;

    public const string InvalidCredentialsErrorMessage = "Invalid username or password";
}
