namespace IdentityServerHost.Quickstart.UI.UnitTests;

// AllowLocalLogin/AllowRememberLogin/AutomaticRedirectAfterSignOut are const,
// not tested here - MSTEST0032 correctly flags asserting against a compile-time
// constant as vacuous, since the assertion can never fail. Only the two fields
// with a genuine runtime-evaluated value are covered.
[TestClass]
public class AccountOptionsTests
{
    [TestMethod]
    public void RememberMeLoginDuration_is_thirty_days()
    {
        Assert.AreEqual(TimeSpan.FromDays(30), AccountOptions.RememberMeLoginDuration);
    }

    // ShowLogoutPrompt is kept static readonly (not const) deliberately, so
    // AccountController's "show logout confirmation" branch stays reachable
    // instead of being provably dead code. If this ever flips to true, that
    // branch needs re-verifying, not just this assertion updating.
    [TestMethod]
    public void ShowLogoutPrompt_is_false()
    {
        Assert.IsFalse(AccountOptions.ShowLogoutPrompt);
    }
}
