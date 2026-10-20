using System.Security.Claims;
using eShop.Ordering.API.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace eShop.Ordering.API.UnitTests;

[TestClass]
public class IdentityServiceTests
{
    private static IHttpContextAccessor CreateAccessor(params Claim[] claims)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims)),
        };
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);
        return accessor;
    }

    [TestMethod]
    public void GetUserIdentity_returns_the_sub_claim_value()
    {
        var service = new IdentityService(CreateAccessor(new Claim("sub", "user-1")));

        Assert.AreEqual("user-1", service.GetUserIdentity());
    }

    [TestMethod]
    public void GetUserName_returns_the_identity_name()
    {
        var service = new IdentityService(CreateAccessor(new Claim(ClaimTypes.Name, "Alice")));

        Assert.AreEqual("Alice", service.GetUserName());
    }
}
