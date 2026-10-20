using System.Diagnostics;
using System.IO.Pipes;
using Duende.IdentityModel.OidcClient.Browser;
using Microsoft.Win32;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace eShop.ClientApp.Desktop;

// MAUI's WebAuthenticator.AuthenticateAsync did all of this for free -- opened a secure browser
// session and transparently captured the eshop://authcallback redirect via OS-level protocol
// activation it registered itself. Avalonia has no equivalent, so this rebuilds the real
// mechanism for Windows Desktop specifically (confirmed working end-to-end on this machine --
// macOS/Linux need the equivalent OS-level registration too, but aren't implemented here: no
// Mac/Linux desktop environment available to build or verify against).
//
// How it works: EnsureProtocolRegistered writes HKCU registry keys so Windows launches this exe
// whenever an eshop:// URL is activated (a browser redirect, a clicked link, etc.). Since the
// app is already running and showing the system browser when that happens, Windows launches a
// SECOND instance with the URL as argv[0] -- Program.cs detects that (via the single-instance
// Mutex), forwards the URL over a named pipe to the FIRST instance, and exits immediately
// without starting Avalonia a second time. The first instance's pipe listener (started here)
// receives it and completes whichever TaskCompletionSource InvokeAsync is currently awaiting.
public class AvaloniaAuthenticationBrowser : IBrowser
{
    public const string PipeName = "eShop.ClientApp.AuthCallback";
    private const string UrlScheme = "eshop";

    private static readonly Lock ListenerLock = new();
    private static bool _listenerStarted;
    private static TaskCompletionSource<string>? _pendingCallback;

    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        EnsureProtocolRegistered();
        EnsureListenerStarted();

        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingCallback = tcs;

        try
        {
            Process.Start(new ProcessStartInfo { FileName = options.StartUrl, UseShellExecute = true });

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

    // Idempotent: only writes when the registered command doesn't already point at this exact
    // exe (a dev build's path changes between machines/checkouts, so this can't be a one-time
    // installer step the way a packaged app's manifest registration would be).
    private static void EnsureProtocolRegistered()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Known, flagged gap: macOS needs an Info.plist CFBundleURLTypes entry, Linux needs
            // a .desktop file with x-scheme-handler/eshop registered via xdg-mime. Neither is
            // implemented -- no Mac/Linux desktop environment available here to build or verify
            // against, unlike Windows.
            return;
        }

        var exePath = Environment.ProcessPath;

        if (string.IsNullOrEmpty(exePath))
        {
            return;
        }

        var command = $"\"{exePath}\" \"%1\"";

        using var scheme = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{UrlScheme}");
        scheme.SetValue(string.Empty, $"URL:{UrlScheme} Protocol");
        scheme.SetValue("URL Protocol", string.Empty);

        using var commandKey = scheme.CreateSubKey(@"shell\open\command");

        if (!string.Equals(commandKey.GetValue(string.Empty) as string, command, StringComparison.Ordinal))
        {
            commandKey.SetValue(string.Empty, command);
        }
    }

    private static void EnsureListenerStarted()
    {
        lock (ListenerLock)
        {
            if (_listenerStarted)
            {
                return;
            }

            _listenerStarted = true;
        }

        _ = Task.Run(ListenForActivationsAsync);
    }

    private static async Task ListenForActivationsAsync()
    {
        while (true)
        {
            try
            {
                await using var server = new NamedPipeServerStream(PipeName, PipeDirection.In);
                await server.WaitForConnectionAsync().ConfigureAwait(false);

                using var reader = new StreamReader(server);
                var url = await reader.ReadLineAsync().ConfigureAwait(false);

                if (!string.IsNullOrEmpty(url))
                {
                    _pendingCallback?.TrySetResult(url);
                }
            }
            catch (IOException)
            {
                // Client disconnected mid-write or similar transient pipe fault -- the loop
                // just re-creates the server and waits for the next activation.
            }
        }
    }
}
