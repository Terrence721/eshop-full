using Avalonia.Media;

namespace eShop.ClientApp.Services.Theme
{
    // Already a no-op in the real upstream app -- carried over as-is, not something newly stubbed
    // out here. Only the Color parameter type changed, from MAUI's Microsoft.Maui.Graphics.Color
    // to Avalonia's own.
    public class Theme : ITheme
    {
        public void SetStatusBarColor(Color color, bool darkStatusBarTint)
        {
        }
    }
}
