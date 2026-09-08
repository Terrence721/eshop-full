namespace IdentityServerHost.Quickstart.UI.UnitTests;

[TestClass]
public class ProcessConsentResultTests
{
    [TestMethod]
    public void IsRedirect_false_when_RedirectUri_not_set()
    {
        var result = new ProcessConsentResult<ConsentViewModel>();

        Assert.IsFalse(result.IsRedirect);
    }

    [TestMethod]
    public void IsRedirect_true_when_RedirectUri_set()
    {
        var result = new ProcessConsentResult<ConsentViewModel> { RedirectUri = "https://client.example/callback" };

        Assert.IsTrue(result.IsRedirect);
    }

    [TestMethod]
    public void ShowView_false_when_ViewModel_not_set()
    {
        var result = new ProcessConsentResult<ConsentViewModel>();

        Assert.IsFalse(result.ShowView);
    }

    [TestMethod]
    public void ShowView_true_when_ViewModel_set()
    {
        var result = new ProcessConsentResult<ConsentViewModel>
        {
            ViewModel = new ConsentViewModel
            {
                ClientName = "client",
                IdentityScopes = [],
                ApiScopes = []
            }
        };

        Assert.IsTrue(result.ShowView);
    }

    [TestMethod]
    public void HasValidationError_false_when_ValidationError_not_set()
    {
        var result = new ProcessConsentResult<ConsentViewModel>();

        Assert.IsFalse(result.HasValidationError);
    }

    [TestMethod]
    public void HasValidationError_true_when_ValidationError_set()
    {
        var result = new ProcessConsentResult<ConsentViewModel> { ValidationError = "must choose one" };

        Assert.IsTrue(result.HasValidationError);
    }
}
