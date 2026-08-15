using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace eShop.ServiceDefaults.UnitTests;

[TestClass]
public class AuthenticationExtensionsTests
{
    private static HostApplicationBuilder BuilderWithIdentity(string url, string audience)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(
        [
            new("Identity:Url", url),
            new("Identity:Audience", audience),
        ]);
        return builder;
    }

    [TestMethod]
    public void AddDefaultAuthentication_does_not_register_authentication_when_no_Identity_section()
    {
        var builder = Host.CreateApplicationBuilder();
        var countBefore = builder.Services.Count;

        var result = builder.AddDefaultAuthentication();

        Assert.AreSame(builder.Services, result);
        Assert.AreEqual(countBefore, builder.Services.Count);
    }

    [TestMethod]
    public void AddDefaultAuthentication_removes_sub_from_default_inbound_claim_type_map_when_Identity_section_present()
    {
        var builder = BuilderWithIdentity("https://identity.example.test", "basket");

        builder.AddDefaultAuthentication();

        Assert.IsFalse(JsonWebTokenHandler.DefaultInboundClaimTypeMap.ContainsKey("sub"));
    }

    [TestMethod]
    public void AddDefaultAuthentication_registers_JwtBearer_scheme_when_Identity_section_present()
    {
        var builder = BuilderWithIdentity("https://identity.example.test", "basket");

        builder.AddDefaultAuthentication();

        using var provider = builder.Services.BuildServiceProvider();
        var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();
        var scheme = schemeProvider.GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme).GetAwaiter().GetResult();

        Assert.IsNotNull(scheme);
    }

    [TestMethod]
    public void AddDefaultAuthentication_configures_authority_audience_and_issuer_from_configuration()
    {
        var builder = BuilderWithIdentity("https://identity.example.test:5001", "basket");

        builder.AddDefaultAuthentication();

        using var provider = builder.Services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.AreEqual("https://identity.example.test:5001", options.Authority);
        Assert.AreEqual("basket", options.Audience);
        Assert.IsFalse(options.RequireHttpsMetadata);
        CollectionAssert.Contains(options.TokenValidationParameters.ValidIssuers.ToList(), "https://identity.example.test:5001");
    }

#if DEBUG
    [TestMethod]
    public void AddDefaultAuthentication_includes_android_emulator_issuer_derived_from_real_port_in_debug()
    {
        var builder = BuilderWithIdentity("https://identity.example.test:5001", "basket");

        builder.AddDefaultAuthentication();

        using var provider = builder.Services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);

        CollectionAssert.Contains(options.TokenValidationParameters.ValidIssuers.ToList(), "https://10.0.2.2:5001");
    }
#endif
}
