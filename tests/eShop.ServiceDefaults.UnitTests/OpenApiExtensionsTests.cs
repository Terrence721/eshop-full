using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace eShop.ServiceDefaults.UnitTests;

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
    public void UseDefaultOpenApi_returns_app_unchanged_when_no_OpenApi_section()
    {
        using var app = WebApplication.CreateBuilder().Build();

        var result = app.UseDefaultOpenApi();

        Assert.AreSame(app, result);
    }
}
