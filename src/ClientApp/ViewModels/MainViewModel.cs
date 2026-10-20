using eShop.ClientApp.Services;

namespace eShop.ClientApp.ViewModels;

// The real navigation shell -- CurrentViewModel just forwards INavigationService's own
// CurrentViewModel, re-raising PropertyChanged so MainWindow/MainView's ContentControl (bound
// to this property, rendered via the globally-registered ViewLocator) updates whenever
// NavigateToAsync/PopAsync swaps what's current.
public class MainViewModel : ViewModelBase
{
    public MainViewModel(INavigationService navigationService) : base(navigationService)
    {
        navigationService.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
            {
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        };
    }

    public ViewModelBase? CurrentViewModel => NavigationService.CurrentViewModel;

    public override Task InitializeAsync()
    {
        return NavigationService.InitializeAsync();
    }
}
