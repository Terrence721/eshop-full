using System.ComponentModel;
using eShop.ClientApp.ViewModels;

namespace eShop.ClientApp.Services
{
    public interface INavigationService : INotifyPropertyChanged
    {
        ViewModelBase? CurrentViewModel { get; }

        void RegisterRoute(string route, Func<ViewModelBase> factory);

        Task InitializeAsync();

        Task NavigateToAsync(string route, IDictionary<string, object>? routeParameters = null);

        Task PopAsync();
    }
}
