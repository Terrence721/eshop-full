using Microsoft.Extensions.Configuration;

namespace eShop.ServiceDefaults.UnitTests;

[TestClass]
public class ConfigurationExtensionsTests
{
    [TestMethod]
    public void GetRequiredValue_returns_value_when_key_present()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([new("Foo", "bar")])
            .Build();

        var result = configuration.GetRequiredValue("Foo");

        Assert.AreEqual("bar", result);
    }

    [TestMethod]
    public void GetRequiredValue_throws_with_plain_name_when_key_missing_on_root_configuration()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => configuration.GetRequiredValue("Missing"));

        Assert.AreEqual("Configuration missing value for: Missing", exception.Message);
    }

    [TestMethod]
    public void GetRequiredValue_throws_with_section_path_when_key_missing_on_section()
    {
        var configuration = new ConfigurationBuilder().Build();
        var section = configuration.GetSection("Identity");

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => section.GetRequiredValue("Missing"));

        Assert.AreEqual("Configuration missing value for: Identity:Missing", exception.Message);
    }
}
