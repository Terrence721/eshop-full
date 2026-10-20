using System.ComponentModel;
using eShop.ClientApp.ViewModels;

namespace eShop.ClientApp.UnitTests.Mocks;

public class MockNavigationService : INavigationService
{
    public ViewModelBase? CurrentViewModel => null;

#pragma warning disable CS0067
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067

    public void RegisterRoute(string route, Func<ViewModelBase> factory)
    {
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task NavigateToAsync(string route, IDictionary<string, object>? routeParameters = null)
    {
        return Task.CompletedTask;
    }

    public Task PopAsync()
    {
        return Task.CompletedTask;
    }
}
