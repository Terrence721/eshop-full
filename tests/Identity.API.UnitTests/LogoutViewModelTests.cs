namespace IdentityServerHost.Quickstart.UI.UnitTests;

[TestClass]
public class LogoutViewModelTests
{
    [TestMethod]
    public void ShowLogoutPrompt_defaults_to_true()
    {
        var vm = new LogoutViewModel();

        Assert.IsTrue(vm.ShowLogoutPrompt);
    }
}
