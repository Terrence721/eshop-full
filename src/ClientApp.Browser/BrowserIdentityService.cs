using Duende.IdentityModel.OidcClient;
using eShop.ClientApp.Models.Token;
using eShop.ClientApp.Models.User;
using eShop.ClientApp.Services.Identity;
using eShop.ClientApp.Services.Settings;

namespace eShop.ClientApp.Browser;

// Desktop's AvaloniaAuthenticationBrowser implements Duende's IBrowser and plugs into the
// shared IdentityService, because OidcClient's "automatic" LoginAsync(LoginRequest) can
// meaningfully await a whole interactive round trip inside one running process. That doesn't
// hold in a browser tab: WASM has no concept of "launch an external browser and wait for it" --
// the app *is* the browser tab, and a real interactive login necessarily means navigating the
// whole page away to the IdP and back, which reloads and restarts the WASM runtime, destroying
// any in-memory await. Automatic mode is also specifically documented as popup-blocked here,
// since OidcClient does an async discovery-document fetch before showing anything, breaking the
// synchronous-user-gesture requirement most browsers enforce for window.open.
//
// So this doesn't implement IBrowser at all -- it uses OidcClient's "manual mode" instead:
// PrepareLoginAsync() builds the authorize URL and PKCE state without showing anything, the
// pending State/CodeVerifier/RedirectUri are stashed in sessionStorage (the one piece of browser
// storage that survives a full-page navigation to the same origin), then the page is redirected
// to the IdP for real. On the next load, Program.cs checks for a pending login before the
// Avalonia UI even starts, and TryCompletePendingLoginAsync reconstructs the AuthorizeState from
// sessionStorage and finishes the exchange with ProcessResponseAsync.
public class BrowserIdentityService(ISettingsService settingsService) : IIdentityService
{
    private const string PendingStateKey = "eshop.oidc.state";
    private const string PendingVerifierKey = "eshop.oidc.verifier";
    private const string PendingRedirectUriKey = "eshop.oidc.redirect_uri";

    private readonly ISettingsService _settingsService = settingsService;

    public async Task<bool> SignInAsync()
    {
        var redirectUri = StripQuery(BrowserInterop.GetCurrentUrl());
        var client = CreateClient(redirectUri);

        AuthorizeState state;
        try
        {
            // PrepareLoginAsync fetches the IdP's discovery document, so this can fail the same
            // way Desktop's LoginAsync can -- same try/catch shape as IdentityService.SignInAsync,
            // so a connection failure here leaves the button usable again instead of leaving
            // IsBusy stuck true forever on an uncaught exception.
            state = await client.PrepareLoginAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException)
        {
            return false;
        }

        BrowserInterop.SessionStorageSet(PendingStateKey, state.State);
        BrowserInterop.SessionStorageSet(PendingVerifierKey, state.CodeVerifier);
        BrowserInterop.SessionStorageSet(PendingRedirectUriKey, redirectUri);

        // Navigating away ends this call from the app's perspective -- the actual sign-in
        // completes on the next page load, via TryCompletePendingLoginAsync.
        BrowserInterop.NavigateTo(state.StartUrl);

        return true;
    }

    // Not part of IIdentityService: called once from Program.cs, before the Avalonia UI starts,
    // to finish a login that was in progress when the IdP redirected back here.
    public async Task<bool> TryCompletePendingLoginAsync()
    {
        var pendingState = BrowserInterop.SessionStorageGet(PendingStateKey);

        if (pendingState is null)
        {
            return false;
        }

        var currentUrl = BrowserInterop.GetCurrentUrl();
        var uri = new Uri(currentUrl);

        if (string.IsNullOrEmpty(uri.Query) || !uri.Query.Contains("code=", StringComparison.Ordinal))
        {
            return false;
        }

        var redirectUri = BrowserInterop.SessionStorageGet(PendingRedirectUriKey) ?? StripQuery(currentUrl);

        var state = new AuthorizeState
        {
            State = pendingState,
            CodeVerifier = BrowserInterop.SessionStorageGet(PendingVerifierKey),
            RedirectUri = redirectUri
        };

        var client = CreateClient(redirectUri);
        var result = await client.ProcessResponseAsync(currentUrl, state).ConfigureAwait(false);

        BrowserInterop.SessionStorageRemove(PendingStateKey);
        BrowserInterop.SessionStorageRemove(PendingVerifierKey);
        BrowserInterop.SessionStorageRemove(PendingRedirectUriKey);

        // Clean the code/state query string out of the address bar without another reload.
        BrowserInterop.ReplaceUrl(redirectUri);

        if (result.IsError)
        {
            return false;
        }

        await _settingsService
            .SetUserTokenAsync(
                new UserToken
                {
                    AccessToken = result.AccessToken,
                    IdToken = result.IdentityToken,
                    RefreshToken = result.RefreshToken,
                    ExpiresAt = result.AccessTokenExpiration
                })
            .ConfigureAwait(false);

        return true;
    }

    public async Task<bool> SignOutAsync()
    {
        // A real front-channel logout needs the same "redirect away and reconstruct state on
        // return" dance as sign-in (OidcClient.LogoutAsync is just as browser-interactive as
        // LoginAsync). Clearing the locally-held token is enough to make the app treat the user
        // as signed out -- a deliberate simplification, not a silent gap, since a full IdP
        // session teardown adds real complexity for very little practical difference here.
        await _settingsService.SetUserTokenAsync(null).ConfigureAwait(false);

        return true;
    }

    public async Task<UserInfo> GetUserInfoAsync()
    {
        var authToken = await GetAuthTokenAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(authToken))
        {
            return UserInfo.Default;
        }

        var client = CreateClient(StripQuery(BrowserInterop.GetCurrentUrl()));
        var userInfoWithClaims = await client.GetUserInfoAsync(authToken).ConfigureAwait(false);

        return
            new UserInfo
            {
                UserId = userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "sub")?.Value!,
                Email = userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "email")?.Value,
                PhoneNumber = userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "phone_number")?.Value,
                Street = userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "address_street")?.Value,
                Address = userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "address_city")?.Value,
                State = userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "address_state")?.Value,
                ZipCode = userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "address_zip_code")?.Value,
                Country = userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "address_country")?.Value,
                PreferredUsername =
                    userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value!,
                Name = userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "name")?.Value,
                LastName = userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "last_name")?.Value,
                CardNumber = userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "card_number")?.Value,
                CardHolder = userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "card_holder")?.Value,
                CardSecurityNumber =
                    userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "card_security_number")?.Value,
                PhoneNumberVerified =
                    bool.Parse(userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "phone_number_verified")
                        ?.Value ?? "false"),
                EmailVerified =
                    bool.Parse(userInfoWithClaims.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value ??
                               "false")
            };
    }

    public async Task<string> GetAuthTokenAsync()
    {
        var userToken = await _settingsService.GetUserTokenAsync().ConfigureAwait(false);

        if (userToken is null)
        {
            return string.Empty;
        }

        if (userToken.ExpiresAt.Subtract(DateTimeOffset.Now).TotalMinutes > 5)
        {
            return userToken.AccessToken;
        }

        var client = CreateClient(StripQuery(BrowserInterop.GetCurrentUrl()));
        var response = await client.RefreshTokenAsync(userToken.RefreshToken).ConfigureAwait(false);

        if (response.IsError)
        {
            return string.Empty;
        }

        await _settingsService
            .SetUserTokenAsync(
                new UserToken
                {
                    AccessToken = response.AccessToken,
                    IdToken = response.IdentityToken,
                    RefreshToken = response.RefreshToken,
                    ExpiresAt = response.AccessTokenExpiration
                })
            .ConfigureAwait(false);

        return response.AccessToken;
    }

    private static string StripQuery(string url)
    {
        return new Uri(url).GetLeftPart(UriPartial.Path);
    }

    private OidcClient CreateClient(string redirectUri)
    {
        var options = new OidcClientOptions
        {
            Authority = _settingsService.IdentityEndpointBase,
            ClientId = _settingsService.ClientId,
            ClientSecret = _settingsService.ClientSecret,
            Scope = "openid profile basket orders offline_access",
            RedirectUri = redirectUri,
            PostLogoutRedirectUri = redirectUri
        };

        return new OidcClient(options);
    }
}
