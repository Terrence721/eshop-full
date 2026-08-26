namespace IdentityServerHost.Quickstart.UI.UnitTests;

[TestClass]
public class LoggedOutViewModelTests
{
    [TestMethod]
    public void TriggerExternalSignout_false_when_ExternalAuthenticationScheme_not_set()
    {
        var vm = new LoggedOutViewModel();

        Assert.IsFalse(vm.TriggerExternalSignout);
    }

    [TestMethod]
    public void TriggerExternalSignout_true_when_ExternalAuthenticationScheme_set()
    {
        var vm = new LoggedOutViewModel { ExternalAuthenticationScheme = "google" };

        Assert.IsTrue(vm.TriggerExternalSignout);
    }
}
