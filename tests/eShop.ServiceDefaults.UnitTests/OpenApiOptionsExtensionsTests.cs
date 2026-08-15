using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Primitives;

namespace eShop.ServiceDefaults.UnitTests;

[TestClass]
public class OpenApiOptionsExtensionsTests
{
    [TestMethod]
    public void AppendSentenceSeparator_does_nothing_when_text_is_empty()
    {
        var text = new StringBuilder();

        OpenApiOptionsExtensions.AppendSentenceSeparator(text);

        Assert.AreEqual(string.Empty, text.ToString());
    }

    [TestMethod]
    public void AppendSentenceSeparator_only_adds_space_when_text_already_ends_with_period()
    {
        var text = new StringBuilder("Already ends in a period.");

        OpenApiOptionsExtensions.AppendSentenceSeparator(text);

        Assert.AreEqual("Already ends in a period. ", text.ToString());
    }

    [TestMethod]
    public void AppendSentenceSeparator_adds_period_and_space_when_text_does_not_end_with_period()
    {
        var text = new StringBuilder("No trailing period");

        OpenApiOptionsExtensions.AppendSentenceSeparator(text);

        Assert.AreEqual("No trailing period. ", text.ToString());
    }

    [TestMethod]
    public void BuildDescription_returns_plain_description_when_not_deprecated_and_no_sunset_policy()
    {
        var api = new ApiVersionDescription(new ApiVersion(1, 0), "v1");

        var result = OpenApiOptionsExtensions.BuildDescription(api, "Catalog API");

        Assert.AreEqual("Catalog API", result);
    }

    [TestMethod]
    public void BuildDescription_appends_deprecation_notice_when_deprecated()
    {
        var api = new ApiVersionDescription(new ApiVersion(1, 0), "v1", deprecated: true);

        var result = OpenApiOptionsExtensions.BuildDescription(api, "Catalog API");

        Assert.AreEqual("Catalog API. This API version has been deprecated.", result);
    }

    [TestMethod]
    public void BuildDescription_does_not_double_period_when_description_already_ends_with_one()
    {
        var api = new ApiVersionDescription(new ApiVersion(1, 0), "v1", deprecated: true);

        var result = OpenApiOptionsExtensions.BuildDescription(api, "Catalog API.");

        Assert.AreEqual("Catalog API. This API version has been deprecated.", result);
    }

    [TestMethod]
    public void BuildDescription_appends_sunset_date_when_policy_has_a_date()
    {
        var sunsetDate = new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var api = new ApiVersionDescription(new ApiVersion(1, 0), "v1", sunsetPolicy: new SunsetPolicy(sunsetDate));

        var result = OpenApiOptionsExtensions.BuildDescription(api, "Catalog API");

        var expectedDate = sunsetDate.Date.ToShortDateString();
        Assert.AreEqual($"Catalog API. The API will be sunset on {expectedDate}.", result);
    }

    [TestMethod]
    public void BuildDescription_renders_only_html_links_with_title_when_available()
    {
        var htmlLinkWithTitle = new LinkHeaderValue(new Uri("https://example.test/deprecation-policy"), "sunset")
        {
            Type = "text/html",
            Title = "Deprecation policy",
        };
        var htmlLinkWithoutTitle = new LinkHeaderValue(new Uri("https://example.test/notice"), "sunset")
        {
            Type = "text/html",
        };
        var nonHtmlLink = new LinkHeaderValue(new Uri("https://example.test/notice.json"), "sunset")
        {
            Type = "application/json",
        };
        var policy = new SunsetPolicy();
        foreach (var link in new[] { htmlLinkWithTitle, htmlLinkWithoutTitle, nonHtmlLink })
        {
            policy.Links.Add(link);
        }

        var api = new ApiVersionDescription(new ApiVersion(1, 0), "v1", sunsetPolicy: policy);

        var result = OpenApiOptionsExtensions.BuildDescription(api, "Catalog API");

        StringAssert.Contains(result, "<h4>Links</h4><ul>");
        StringAssert.Contains(result, "<li><a href=\"https://example.test/deprecation-policy\">Deprecation policy</a></li>");
        StringAssert.Contains(result, "<li><a href=\"https://example.test/notice\">https://example.test/notice</a></li>");
        StringAssert.DoesNotMatch(result, new System.Text.RegularExpressions.Regex("notice\\.json"));
    }

    [TestMethod]
    public void BuildDescription_combines_deprecation_and_sunset_date_with_correct_separators()
    {
        var sunsetDate = new DateTimeOffset(2027, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var api = new ApiVersionDescription(new ApiVersion(1, 0), "v1", deprecated: true, sunsetPolicy: new SunsetPolicy(sunsetDate));

        var result = OpenApiOptionsExtensions.BuildDescription(api, "Catalog API");

        var expectedDate = sunsetDate.Date.ToShortDateString();
        Assert.AreEqual(
            $"Catalog API. This API version has been deprecated. The API will be sunset on {expectedDate}.",
            result);
    }
}
