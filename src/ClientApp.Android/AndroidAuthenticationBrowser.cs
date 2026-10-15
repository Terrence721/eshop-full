using Android.Content;
using Duende.IdentityModel.OidcClient.Browser;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;
using AndroidApplication = Android.App.Application;

namespace eShop.ClientApp.Android;

// Same role as Desktop's AvaloniaAuthenticationBrowser, rebuilt for Android's own redirect
// mechanism instead of Windows' registry protocol registration + named pipe: Android already
// routes a custom-scheme URL back into this app via a plain manifest intent-filter (declared on
// AuthCallbackActivity below) -- no OS-level registration step needed, since the package manager
// handles that from the manifest at install time. Launching a bare ActionView Intent (rather than
// AndroidX Browser's CustomTabsIntent) avoids a new package dependency for a one-off redirect:
// the system browser is enough, same tradeoff Desktop made with Process.Start.
public class AndroidAuthenticationBrowser : IBrowser
{
    private static TaskCompletionSource<string>? _pendingCallback;

    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingCallback = tcs;

        try
        {
            var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(options.StartUrl));
            intent.AddFlags(ActivityFlags.NewTask);
            AndroidApplication.Context.StartActivity(intent);

            using var timeoutCts = new CancellationTokenSource(options.Timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var callbackUrl = await tcs.Task.WaitAsync(linkedCts.Token).ConfigureAwait(false);

            return new BrowserResult { Response = callbackUrl, ResultType = BrowserResultType.Success };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new BrowserResult { ResultType = BrowserResultType.UserCancel };
        }
        catch (OperationCanceledException)
        {
            return new BrowserResult { ResultType = BrowserResultType.Timeout };
        }
        finally
        {
            _pendingCallback = null;
        }
    }

    // Called by AuthCallbackActivity when the IdP redirects back to eshop://authcallback.
    internal static void CompleteLogin(string url)
    {
        _pendingCallback?.TrySetResult(url);
    }
}
