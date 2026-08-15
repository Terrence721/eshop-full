using System.Security.Claims;

namespace eShop.ServiceDefaults.UnitTests;

[TestClass]
public class ClaimsPrincipalExtensionsTests
{
    private static ClaimsPrincipal PrincipalWith(params Claim[] claims) =>
        new(new ClaimsIdentity(claims));

    [TestMethod]
    public void GetUserId_returns_sub_claim_when_present()
    {
        var principal = PrincipalWith(new Claim("sub", "sub-value"));

        Assert.AreEqual("sub-value", principal.GetUserId());
    }

    [TestMethod]
    public void GetUserId_falls_back_to_NameIdentifier_when_sub_missing()
    {
        var principal = PrincipalWith(new Claim(ClaimTypes.NameIdentifier, "nameid-value"));

        Assert.AreEqual("nameid-value", principal.GetUserId());
    }

    [TestMethod]
    public void GetUserId_prefers_sub_over_NameIdentifier_when_both_present()
    {
        var principal = PrincipalWith(
            new Claim("sub", "sub-value"),
            new Claim(ClaimTypes.NameIdentifier, "nameid-value"));

        Assert.AreEqual("sub-value", principal.GetUserId());
    }

    [TestMethod]
    public void GetUserId_returns_null_when_neither_claim_present()
    {
        var principal = PrincipalWith();

        Assert.IsNull(principal.GetUserId());
    }

    [TestMethod]
    public void GetUserName_returns_Name_claim_when_present()
    {
        var principal = PrincipalWith(new Claim(ClaimTypes.Name, "name-value"));

        Assert.AreEqual("name-value", principal.GetUserName());
    }

    [TestMethod]
    public void GetUserName_returns_null_when_Name_claim_missing()
    {
        var principal = PrincipalWith();

        Assert.IsNull(principal.GetUserName());
    }
}
