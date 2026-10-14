using eShop.ClientApp.Services.Identity;

namespace eShop.ClientApp.Services.AppEnvironment;

public class AppEnvironmentService(IIdentityService mockIdentityService, IIdentityService identityService)
    : IAppEnvironmentService
{
    private readonly IIdentityService _mockIdentityService = mockIdentityService;
    private readonly IIdentityService _identityService = identityService;

    public IIdentityService IdentityService { get; private set; } = null!;

    public void UpdateDependencies(bool useMockServices)
    {
        IdentityService = useMockServices ? _mockIdentityService : _identityService;
    }
}
