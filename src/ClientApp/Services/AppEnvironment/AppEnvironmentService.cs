using eShop.ClientApp.Services.Basket;
using eShop.ClientApp.Services.Catalog;
using eShop.ClientApp.Services.Identity;
using eShop.ClientApp.Services.Order;

namespace eShop.ClientApp.Services.AppEnvironment
{
    public class AppEnvironmentService(
        IBasketService mockBasketService, IBasketService basketService,
        ICatalogService mockCatalogService, ICatalogService catalogService,
        IOrderService mockOrderService, IOrderService orderService,
        IIdentityService mockIdentityService, IIdentityService identityService)
        : IAppEnvironmentService
    {
        private readonly IBasketService _mockBasketService = mockBasketService;
        private readonly IBasketService _basketService = basketService;
        private readonly ICatalogService _mockCatalogService = mockCatalogService;
        private readonly ICatalogService _catalogService = catalogService;
        private readonly IOrderService _mockOrderService = mockOrderService;
        private readonly IOrderService _orderService = orderService;
        private readonly IIdentityService _mockIdentityService = mockIdentityService;
        private readonly IIdentityService _identityService = identityService;

        public IBasketService BasketService { get; private set; } = null!;

        public ICatalogService CatalogService { get; private set; } = null!;

        public IOrderService OrderService { get; private set; } = null!;

        public IIdentityService IdentityService { get; private set; } = null!;

        public void UpdateDependencies(bool useMockServices)
        {
            if (useMockServices)
            {
                BasketService = _mockBasketService;
                CatalogService = _mockCatalogService;
                OrderService = _mockOrderService;
                IdentityService = _mockIdentityService;
            }
            else
            {
                BasketService = _basketService;
                CatalogService = _catalogService;
                OrderService = _orderService;
                IdentityService = _identityService;
            }
        }
    }
}
