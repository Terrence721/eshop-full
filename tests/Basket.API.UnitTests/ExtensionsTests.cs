using eShop.Basket.API.Extensions;
using eShop.Basket.API.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace eShop.Basket.API.UnitTests
{
    [TestClass]
    public class ExtensionsTests
    {
        private static IHostApplicationBuilder CreateBuilder()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Configuration["ConnectionStrings:redis"] = "localhost:6379";
            builder.Configuration["ConnectionStrings:eventbus"] = "localhost:9092";
            return builder;
        }

        [TestMethod]
        public void AddApplicationServices_registers_the_basket_repository()
        {
            var builder = CreateBuilder();

            builder.AddApplicationServices();

            var serviceProvider = builder.Services.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider.GetService<IBasketRepository>());
        }

        [TestMethod]
        public void AddApplicationServices_does_not_throw_with_configured_endpoints()
        {
            var builder = CreateBuilder();

            builder.AddApplicationServices();

            var serviceProvider = builder.Services.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider);
        }
    }
}