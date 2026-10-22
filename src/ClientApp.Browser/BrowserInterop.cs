using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace eShop.ClientApp.Browser
{
    // Thin wrapper around wwwroot/js/interop.js, loaded once via JSHost.ImportAsync in Program.cs
    // before anything here is called. Avalonia Browser apps have no Blazor/Razor runtime -- this is
    // .NET's own [JSImport]/[JSExport] WASM interop (System.Runtime.InteropServices.JavaScript),
    // which works in any .NET WebAssembly app, not something specific to this project.
    [SupportedOSPlatform("browser")]
    internal static partial class BrowserInterop
    {
        [JSImport("getCurrentUrl", "interop")]
        public static partial string GetCurrentUrl();

        [JSImport("navigateTo", "interop")]
        public static partial void NavigateTo(string url);

        [JSImport("replaceUrl", "interop")]
        public static partial void ReplaceUrl(string url);

        [JSImport("sessionStorageGet", "interop")]
        public static partial string? SessionStorageGet(string key);

        [JSImport("sessionStorageSet", "interop")]
        public static partial void SessionStorageSet(string key, string value);

        [JSImport("sessionStorageRemove", "interop")]
        public static partial void SessionStorageRemove(string key);
    }
}
