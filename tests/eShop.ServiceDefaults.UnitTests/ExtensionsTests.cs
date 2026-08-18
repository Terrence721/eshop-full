using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace eShop.ServiceDefaults.UnitTests;

[TestClass]
public class ExtensionsTests
{
    [TestMethod]
    public async Task AddDefaultHealthChecks_registers_self_check_tagged_live_and_healthy()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.AddDefaultHealthChecks();

        using var provider = builder.Services.BuildServiceProvider();
        var report = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync();

        Assert.IsTrue(report.Entries.ContainsKey("self"));
        var entry = report.Entries["self"];
        Assert.AreEqual(HealthStatus.Healthy, entry.Status);
        CollectionAssert.Contains(entry.Tags.ToList(), "live");
    }

    [TestMethod]
    public void AddServiceDefaults_returns_the_same_builder_instance()
    {
        var builder = Host.CreateApplicationBuilder();

        var result = builder.AddServiceDefaults();

        Assert.AreSame(builder, result);
    }

    [TestMethod]
    public async Task AddServiceDefaults_registers_health_checks_via_AddBasicServiceDefaults()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.AddServiceDefaults();

        using var provider = builder.Services.BuildServiceProvider();
        var report = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync();

        Assert.IsTrue(report.Entries.ContainsKey("self"));
    }

    [TestMethod]
    public void ConfigureOpenTelemetry_registers_a_resolvable_TracerProvider_and_MeterProvider()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.ConfigureOpenTelemetry();

        using var provider = builder.Services.BuildServiceProvider();

        Assert.IsNotNull(provider.GetRequiredService<TracerProvider>());
        Assert.IsNotNull(provider.GetRequiredService<MeterProvider>());
    }
}
