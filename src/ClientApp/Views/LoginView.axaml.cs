using Avalonia.Controls;

namespace eShop.ClientApp.Views;

// Upstream's OnAppearing() did a MAUI-specific content-refresh rendering hack (not needed in
// Avalonia) and called InvalidateMock(), which only ever sets IsMock back to its own
// hardcoded false (the real assignment is commented out in LoginViewModel), so it's a
// provable no-op. Nothing real left to port into a lifecycle hook here.
public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }
}
