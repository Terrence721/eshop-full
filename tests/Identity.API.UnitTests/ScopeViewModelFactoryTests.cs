using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;

namespace IdentityServerHost.Quickstart.UI.UnitTests;

[TestClass]
public class ScopeViewModelFactoryTests
{
    [TestMethod]
    public void CreateScopeViewModel_for_identity_resource_uses_display_name_fallback()
    {
        var identity = new IdentityResource { Name = "profile", DisplayName = null, Required = false };

        var vm = ScopeViewModelFactory.CreateScopeViewModel(identity, check: false);

        Assert.AreEqual("profile", vm.DisplayName);
    }

    [TestMethod]
    public void CreateScopeViewModel_for_api_scope_appends_parsed_parameter_to_display_name()
    {
        // Regression test: DeviceController's copy of this logic used to omit the
        // parsed-parameter suffix that ConsentController's copy included, a real
        // upstream inconsistency (present in Duende's own Quickstart sample too)
        // this extraction fixes by giving both controllers one shared implementation.
        var apiScope = new ApiScope("resource1.scope1") { DisplayName = "Resource 1" };
        var parsedScopeValue = new ParsedScopeValue("resource1.scope1:42", "resource1.scope1", "42");

        var vm = ScopeViewModelFactory.CreateScopeViewModel(parsedScopeValue, apiScope, check: false);

        Assert.AreEqual("Resource 1:42", vm.DisplayName);
    }

    [TestMethod]
    public void CreateScopeViewModel_for_api_scope_without_parsed_parameter_has_no_suffix()
    {
        var apiScope = new ApiScope("resource1.scope1") { DisplayName = "Resource 1" };
        var parsedScopeValue = new ParsedScopeValue("resource1.scope1");

        var vm = ScopeViewModelFactory.CreateScopeViewModel(parsedScopeValue, apiScope, check: false);

        Assert.AreEqual("Resource 1", vm.DisplayName);
    }

    [TestMethod]
    public void GetOfflineAccessScope_is_always_emphasized()
    {
        var vm = ScopeViewModelFactory.GetOfflineAccessScope(check: true);

        Assert.IsTrue(vm.Emphasize);
        Assert.IsTrue(vm.Checked);
    }
}
