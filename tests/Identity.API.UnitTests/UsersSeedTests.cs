using eShop.Identity.API;
using eShop.Identity.API.Data;
using eShop.Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.Identity.API.UnitTests;

[TestClass]
public class UsersSeedTests
{
    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(store, null, null, null, null, null, null, null, null);
    }

    [TestMethod]
    public async Task SeedAsync_creates_every_seed_user_when_none_exist_yet()
    {
        var userManager = CreateUserManager();
        userManager.FindByNameAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        var seeder = new UsersSeed(Substitute.For<ILogger<UsersSeed>>(), userManager);

        await seeder.SeedAsync(null!);

        await userManager.Received(10).CreateAsync(Arg.Any<ApplicationUser>(), "Pass123$");
    }

    [TestMethod]
    public async Task SeedAsync_does_not_recreate_users_that_already_exist()
    {
        var userManager = CreateUserManager();
        userManager.FindByNameAsync(Arg.Any<string>()).Returns(new ApplicationUser { UserName = "alice" });
        var seeder = new UsersSeed(Substitute.For<ILogger<UsersSeed>>(), userManager);

        await seeder.SeedAsync(null!);

        await userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [TestMethod]
    public async Task SeedAsync_throws_when_CreateAsync_fails()
    {
        var userManager = CreateUserManager();
        userManager.FindByNameAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Description = "weak password" }));
        var seeder = new UsersSeed(Substitute.For<ILogger<UsersSeed>>(), userManager);

        await Assert.ThrowsExactlyAsync<Exception>(() => seeder.SeedAsync(null!));
    }
}
