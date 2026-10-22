using Avalonia.Media;

namespace eShop.ClientApp.Services.Theme
{
    public interface ITheme
    {
        void SetStatusBarColor(Color color, bool darkStatusBarTint);
    }
}
