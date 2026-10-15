using AuthenticationServices;
using Duende.IdentityModel.OidcClient.Browser;
using Foundation;
using UIKit;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace eShop.ClientApp.iOS;

// iOS has its own purpose-built API for exactly this -- ASWebAuthenticationSession -- rather than
// Desktop's registry+named-pipe dance or Android's manifest intent-filter+redirect Activity.
// It's a system-provided, sandboxed browser session scoped to one OAuth/OIDC round trip: passing
// it the callback URL scheme (the same "eshop" scheme Desktop and Android both already use) is
// enough for it to recognize and capture the IdP's redirect itself and hand the full callback URL
// back through its own completion handler, with no Info.plist CFBundleURLTypes registration and
// no custom AppDelegate.OpenUrl handling needed -- unlike the manual custom-scheme interception
// every other platform here needs.
//
// Two constructor generations exist and neither alone covers this app's SupportedOSPlatformVersion
// 13.0 floor: the plain-string-scheme one works from 13.0 but is obsoleted at 17.4+ (real CA1422
// build error, not guessed), while its ASWebAuthenticationSessionCallback-based replacement only
// exists from 17.4 onward (real CA1416 build error the other way). So this branches on the actual
// running OS version instead of picking one, same as any other API that moved version boundaries.
public class AuthenticationSessionBrowser(AppDelegate appDelegate) : IBrowser
{
    private const string UrlScheme = "eshop";

    public Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<BrowserResult>();
        var url = new NSUrl(options.StartUrl);
        ASWebAuthenticationSessionCompletionHandler onComplete = (callbackUrl, error) =>
        {
            if (error is not null || callbackUrl is null)
            {
                tcs.TrySetResult(new BrowserResult { ResultType = BrowserResultType.UserCancel });
                return;
            }

            tcs.TrySetResult(new BrowserResult { ResultType = BrowserResultType.Success, Response = callbackUrl.AbsoluteString! });
        };

        var session = OperatingSystem.IsIOSVersionAtLeast(17, 4)
            ? new ASWebAuthenticationSession(url, ASWebAuthenticationSessionCallback.Create(UrlScheme), onComplete)
            : new ASWebAuthenticationSession(url, UrlScheme, onComplete);

        session.PresentationContextProvider = new PresentationContextProvider(appDelegate);

        using var registration = cancellationToken.Register(session.Cancel);

        if (!session.Start())
        {
            tcs.TrySetResult(new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = "ASWebAuthenticationSession failed to start." });
        }

        return tcs.Task;
    }

    // ASWebAuthenticationSession needs a UIWindow to present itself over -- AppDelegate.Window
    // (set once Avalonia's own startup creates it) is the same window the rest of the app renders
    // into, so the auth sheet appears over the actual running UI instead of a disconnected one.
    private sealed class PresentationContextProvider(AppDelegate appDelegate)
        : NSObject, IASWebAuthenticationPresentationContextProviding
    {
        // Window is set by Avalonia's own startup before CustomizeAppBuilder returns (see
        // AppDelegate), and a sign-in can only be triggered from already-rendered UI, so it's
        // always non-null by the time this runs despite the property's nullable signature.
        public UIWindow GetPresentationAnchor(ASWebAuthenticationSession session) => appDelegate.Window!;
    }
}
