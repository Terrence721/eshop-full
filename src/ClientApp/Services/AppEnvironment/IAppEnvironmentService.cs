using eShop.ClientApp.Services.Identity;

namespace eShop.ClientApp.Services.AppEnvironment;

// Grows to cover Basket/Catalog/Order the same way once those services exist -- Identity is
// the only mock/live pair built so far, and it's the one thing AvaloniaNavigationService's
// InitializeAsync login check actually needs today.
public interface IAppEnvironmentService
{
    IIdentityService IdentityService { get; }

    void UpdateDependencies(bool useMockServices);
}
