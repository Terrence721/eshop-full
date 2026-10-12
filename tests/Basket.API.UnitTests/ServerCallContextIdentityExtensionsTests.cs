using System.Security.Claims;
using eShop.Basket.API.Extensions;
using Microsoft.AspNetCore.Http;

namespace eShop.Basket.API.UnitTests;

[TestClass]
public class ServerCallContextIdentityExtensionsTests
{
    private static HttpContext CreateHttpContext(params Claim[] claims)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        return httpContext;
    }

    [TestMethod]
    public void GetUserIdentity_returns_the_sub_claim_value()
    {
        var context = TestServerCallContext.Create(CreateHttpContext(new Claim("sub", "alice")));

        Assert.AreEqual("alice", context.GetUserIdentity());
    }

    [TestMethod]
    public void GetUserIdentity_returns_null_when_no_sub_claim()
    {
        var context = TestServerCallContext.Create(CreateHttpContext());

        Assert.IsNull(context.GetUserIdentity());
    }

    [TestMethod]
    public void GetUserName_returns_the_name_claim_value()
    {
        var context = TestServerCallContext.Create(CreateHttpContext(new Claim(ClaimTypes.Name, "Alice")));

        Assert.AreEqual("Alice", context.GetUserName());
    }

    [TestMethod]
    public void GetUserName_returns_null_when_no_name_claim()
    {
        var context = TestServerCallContext.Create(CreateHttpContext());

        Assert.IsNull(context.GetUserName());
    }
}
