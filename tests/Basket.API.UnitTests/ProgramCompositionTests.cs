using eShop.Basket.API.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace eShop.Basket.API.UnitTests
{
    [TestClass]
    public class ProgramCompositionTests
    {
        [TestMethod]
        public void Composition_root_registers_basket_repository()
        {
            using var factory = new WebApplicationFactory<Program>();
            var serviceProvider = factory.Services;
            Assert.IsNotNull(serviceProvider.GetService<IBasketRepository>());
        }
    }
}
