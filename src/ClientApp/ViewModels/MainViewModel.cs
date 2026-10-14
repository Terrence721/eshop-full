using CommunityToolkit.Mvvm.ComponentModel;

namespace eShop.ClientApp.ViewModels;

// Scaffold placeholder — proves the solution closes end-to-end. Replaced by the real
// navigation shell ViewModel (CurrentViewModel + tab/flyout state) in the UI step.
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to eShop!";
}
