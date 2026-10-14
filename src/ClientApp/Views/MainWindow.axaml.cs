using Avalonia.Controls;
using Avalonia.Platform;

namespace eShop.ClientApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://ClientApp/Assets/app-icon-draft.png")));
    }
}
