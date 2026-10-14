using System.ComponentModel;
using eShop.ClientApp.Models.User;
using eShop.ClientApp.Services.AppEnvironment;
using eShop.ClientApp.ViewModels;

namespace eShop.ClientApp.Services;

// Avalonia has no Shell to navigate through, so this replaces MAUI's Shell.GoToAsync with a
// plain ViewModel-first swap: CurrentViewModel is what the shell's root ContentControl binds
// to (rendered via ViewLocator), and routes are registered externally -- mirroring how
// AppShell.xaml.cs's InitializeRouting() used to call Routing.RegisterRoute rather than
// MauiNavigationService knowing about routes itself -- so this stays buildable before any
// ViewModel it will eventually route to exists.
public class AvaloniaNavigationService(IAppEnvironmentService appEnvironmentService) : INavigationService
{
    private readonly IAppEnvironmentService _appEnvironmentService = appEnvironmentService;
    private readonly Dictionary<string, Func<ViewModelBase>> _routes = new();
    private readonly Stack<ViewModelBase> _backStack = new();

    private ViewModelBase? _currentViewModel;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            _currentViewModel = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentViewModel)));
        }
    }

    public void RegisterRoute(string route, Func<ViewModelBase> factory)
    {
        _routes[route] = factory;
    }

    public async Task InitializeAsync()
    {
        var user = await _appEnvironmentService.IdentityService.GetUserInfoAsync();

        await NavigateToAsync(user == UserInfo.Default ? "//Login" : "//Main/Catalog");
    }

    public Task NavigateToAsync(string route, IDictionary<string, object>? routeParameters = null)
    {
        var isAbsolute = route.StartsWith("//", StringComparison.Ordinal);
        var destination = Resolve(route[(route.LastIndexOf('/') + 1)..]);

        if (routeParameters is not null && destination is IQueryAttributable queryAttributable)
        {
            queryAttributable.ApplyQueryAttributes(routeParameters);
        }

        if (isAbsolute)
        {
            _backStack.Clear();
        }
        else if (CurrentViewModel is not null)
        {
            _backStack.Push(CurrentViewModel);
        }

        CurrentViewModel = destination;

        return Task.CompletedTask;
    }

    public Task PopAsync()
    {
        if (_backStack.Count > 0)
        {
            CurrentViewModel = _backStack.Pop();
        }

        return Task.CompletedTask;
    }

    private ViewModelBase Resolve(string routeKey)
    {
        if (!_routes.TryGetValue(routeKey, out var factory))
        {
            throw new KeyNotFoundException($"No route registered for '{routeKey}'.");
        }

        return factory();
    }
}
