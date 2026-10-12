using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace eShop.Catalog.API.UnitTests;

[TestClass]
public class HostEnvironmentExtensionsTests
{
    [TestMethod]
    public void IsBuild_false_for_Development_environment()
    {
        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns("Development");

        Assert.IsFalse(env.IsBuild());
    }

    [TestMethod]
    public void IsBuild_true_for_Build_environment()
    {
        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns("Build");

        Assert.IsTrue(env.IsBuild());
    }
}
