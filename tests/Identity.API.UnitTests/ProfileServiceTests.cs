using System.Security.Claims;
using Duende.IdentityServer.Models;
using eShop.Identity.API.Models;
using eShop.Identity.API.Services;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace eShop.Identity.API.Services.UnitTests;

[TestClass]
public class ProfileServiceTests
{
    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(store, null, null, null, null, null, null, null, null);
    }

    private static ClaimsPrincipal SubjectWith(params Claim[] claims) => new(new ClaimsIdentity(claims));

    [TestMethod]
    public async Task GetProfileDataAsync_throws_when_sub_claim_missing()
    {
        var service = new ProfileService(CreateUserManager());
        var context = new ProfileDataRequestContext { Subject = SubjectWith() };

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => service.GetProfileDataAsync(context, CancellationToken.None));
    }

    [TestMethod]
    public async Task GetProfileDataAsync_throws_when_user_not_found()
    {
        var userManager = CreateUserManager();
        userManager.FindByIdAsync("user-1").Returns((ApplicationUser?)null);
        var service = new ProfileService(userManager);
        var context = new ProfileDataRequestContext { Subject = SubjectWith(new Claim("sub", "user-1")) };

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => service.GetProfileDataAsync(context, CancellationToken.None));
    }

    [TestMethod]
    public async Task GetProfileDataAsync_issues_claims_for_the_found_user()
    {
        var userManager = CreateUserManager();
        var user = new ApplicationUser { Id = "user-1", UserName = "alice", Name = "Alice", LastName = "Smith" };
        userManager.FindByIdAsync("user-1").Returns(user);
        var service = new ProfileService(userManager);
        var context = new ProfileDataRequestContext { Subject = SubjectWith(new Claim("sub", "user-1")) };

        await service.GetProfileDataAsync(context, CancellationToken.None);

        Assert.IsTrue(context.IssuedClaims.Any(c => c.Type == "sub" && c.Value == "user-1"));
        Assert.IsTrue(context.IssuedClaims.Any(c => c.Type == "name" && c.Value == "Alice"));
        Assert.IsTrue(context.IssuedClaims.Any(c => c.Type == "last_name" && c.Value == "Smith"));
    }

    [TestMethod]
    public async Task GetProfileDataAsync_falls_back_to_Id_when_UserName_is_null()
    {
        var userManager = CreateUserManager();
        var user = new ApplicationUser { Id = "user-1", UserName = null };
        userManager.FindByIdAsync("user-1").Returns(user);
        var service = new ProfileService(userManager);
        var context = new ProfileDataRequestContext { Subject = SubjectWith(new Claim("sub", "user-1")) };

        await service.GetProfileDataAsync(context, CancellationToken.None);

        Assert.IsTrue(context.IssuedClaims.Any(c => c.Type == "preferred_username" && c.Value == "user-1"));
    }

    [TestMethod]
    public async Task IsActiveAsync_false_when_sub_claim_missing()
    {
        var service = new ProfileService(CreateUserManager());
        var context = new IsActiveContext(SubjectWith(), new Client { ClientId = "test" }, "test");

        await service.IsActiveAsync(context, CancellationToken.None);

        Assert.IsFalse(context.IsActive);
    }

    [TestMethod]
    public async Task IsActiveAsync_false_when_user_not_found()
    {
        var userManager = CreateUserManager();
        userManager.FindByIdAsync("user-1").Returns((ApplicationUser?)null);
        var service = new ProfileService(userManager);
        var context = new IsActiveContext(SubjectWith(new Claim("sub", "user-1")), new Client { ClientId = "test" }, "test");

        await service.IsActiveAsync(context, CancellationToken.None);

        Assert.IsFalse(context.IsActive);
    }

    [TestMethod]
    public async Task IsActiveAsync_true_when_user_found_and_lockout_disabled()
    {
        var userManager = CreateUserManager();
        var user = new ApplicationUser { Id = "user-1", LockoutEnabled = false };
        userManager.FindByIdAsync("user-1").Returns(user);
        userManager.SupportsUserSecurityStamp.Returns(false);
        var service = new ProfileService(userManager);
        var context = new IsActiveContext(SubjectWith(new Claim("sub", "user-1")), new Client { ClientId = "test" }, "test");

        await service.IsActiveAsync(context, CancellationToken.None);

        Assert.IsTrue(context.IsActive);
    }

    [TestMethod]
    public async Task IsActiveAsync_false_when_security_stamp_mismatch()
    {
        var userManager = CreateUserManager();
        var user = new ApplicationUser { Id = "user-1" };
        userManager.FindByIdAsync("user-1").Returns(user);
        userManager.SupportsUserSecurityStamp.Returns(true);
        userManager.GetSecurityStampAsync(user).Returns("current-stamp");
        var service = new ProfileService(userManager);

        var subject = SubjectWith(
            new Claim("sub", "user-1"),
            new Claim("security_stamp", "stale-stamp"));
        var context = new IsActiveContext(subject, new Client { ClientId = "test" }, "test");

        await service.IsActiveAsync(context, CancellationToken.None);

        Assert.IsFalse(context.IsActive);
    }
}
