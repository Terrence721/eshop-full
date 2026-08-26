namespace IdentityServerHost.Quickstart.UI.UnitTests;

[TestClass]
public class LoginViewModelTests
{
    [TestMethod]
    public void VisibleExternalProviders_excludes_providers_with_no_DisplayName()
    {
        var vm = new LoginViewModel
        {
            ExternalProviders =
            [
                new ExternalProvider { AuthenticationScheme = "google", DisplayName = "Google" },
                new ExternalProvider { AuthenticationScheme = "internal-only" }
            ]
        };

        var visible = vm.VisibleExternalProviders.ToList();

        Assert.HasCount(1, visible);
        Assert.AreEqual("google", visible[0].AuthenticationScheme);
    }

    [TestMethod]
    public void VisibleExternalProviders_excludes_providers_with_whitespace_DisplayName()
    {
        var vm = new LoginViewModel
        {
            ExternalProviders = [new ExternalProvider { AuthenticationScheme = "google", DisplayName = "   " }]
        };

        Assert.IsEmpty(vm.VisibleExternalProviders);
    }

    [TestMethod]
    public void IsExternalLoginOnly_false_when_local_login_enabled_even_with_one_provider()
    {
        var vm = new LoginViewModel
        {
            EnableLocalLogin = true,
            ExternalProviders = [new ExternalProvider { AuthenticationScheme = "google" }]
        };

        Assert.IsFalse(vm.IsExternalLoginOnly);
    }

    [TestMethod]
    public void IsExternalLoginOnly_false_when_local_login_disabled_but_multiple_providers()
    {
        var vm = new LoginViewModel
        {
            EnableLocalLogin = false,
            ExternalProviders =
            [
                new ExternalProvider { AuthenticationScheme = "google" },
                new ExternalProvider { AuthenticationScheme = "microsoft" }
            ]
        };

        Assert.IsFalse(vm.IsExternalLoginOnly);
    }

    [TestMethod]
    public void IsExternalLoginOnly_true_when_local_login_disabled_and_exactly_one_provider()
    {
        var vm = new LoginViewModel
        {
            EnableLocalLogin = false,
            ExternalProviders = [new ExternalProvider { AuthenticationScheme = "google" }]
        };

        Assert.IsTrue(vm.IsExternalLoginOnly);
    }

    [TestMethod]
    public void ExternalLoginScheme_returns_the_scheme_when_external_login_only()
    {
        var vm = new LoginViewModel
        {
            EnableLocalLogin = false,
            ExternalProviders = [new ExternalProvider { AuthenticationScheme = "google" }]
        };

        Assert.AreEqual("google", vm.ExternalLoginScheme);
    }

    [TestMethod]
    public void ExternalLoginScheme_null_when_not_external_login_only()
    {
        var vm = new LoginViewModel
        {
            EnableLocalLogin = true,
            ExternalProviders = [new ExternalProvider { AuthenticationScheme = "google" }]
        };

        Assert.IsNull(vm.ExternalLoginScheme);
    }
}
