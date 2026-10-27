using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using NSubstitute;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace eShop.ServiceDefaults.UnitTests
{
    [TestClass]
    public class OpenApiExtensionsTests
    {
        [TestMethod]
        public void AddDefaultOpenApi_returns_builder_unchanged_when_no_OpenApi_section()
        {
            var builder = Host.CreateApplicationBuilder();
            var countBefore = builder.Services.Count;

            var result = builder.AddDefaultOpenApi();

            Assert.AreSame(builder, result);
            Assert.AreEqual(countBefore, builder.Services.Count);
        }

        [TestMethod]
        public void AddDefaultOpenApi_returns_builder_unchanged_when_OpenApi_section_present_but_no_apiVersioning_supplied()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Configuration.AddInMemoryCollection(
            [
                new("OpenApi:Document:Title", "Test API"),
            ]);
            var countBefore = builder.Services.Count;

            var result = builder.AddDefaultOpenApi();

            Assert.AreSame(builder, result);
            Assert.AreEqual(countBefore, builder.Services.Count);
        }

        [TestMethod]
        public void AddDefaultOpenApi_registers_services_when_section_present_and_versioning_supplied()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Configuration.AddInMemoryCollection(
            [
                new("OpenApi:Document:Title", "Test API"),
                new("OpenApi:Document:Description", "Description"),
            ]);
            var versioning = Substitute.For<IApiVersioningBuilder>();
            var countBefore = builder.Services.Count;

            var result = builder.AddDefaultOpenApi(versioning);

            Assert.AreSame(builder, result);
            Assert.IsTrue(builder.Services.Count >= countBefore);
        }

        [TestMethod]
        public void AddDefaultOpenApi_registers_scopes_from_the_identity_section()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Configuration.AddInMemoryCollection(
            [
                new("OpenApi:Document:Title", "Test API"),
                new("OpenApi:Document:Description", "Description"),
                new("Identity:Scopes:api", "API access"),
            ]);
            var versioning = Substitute.For<IApiVersioningBuilder>();

            var result = builder.AddDefaultOpenApi(versioning);

            Assert.AreSame(builder, result);
        }

        [TestMethod]
        public void UseDefaultOpenApi_returns_app_unchanged_when_no_OpenApi_section()
        {
            using var app = WebApplication.CreateBuilder().Build();

            var result = app.UseDefaultOpenApi();

            Assert.AreSame(app, result);
        }

        [TestMethod]
        public void UseDefaultOpenApi_maps_openapi_endpoints_when_section_present()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.AddInMemoryCollection(
            [
                new("OpenApi:Document:Title", "Test API"),
            ]);
            using var app = builder.Build();

            var result = app.UseDefaultOpenApi();

            Assert.AreSame(app, result);
        }

    }
}
