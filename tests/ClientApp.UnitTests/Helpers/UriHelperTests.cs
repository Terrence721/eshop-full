using eShop.ClientApp.Helpers;

namespace eShop.ClientApp.UnitTests.Helpers
{
    [TestClass]
    public class UriHelperTests
    {
        [TestMethod]
        public void CombineUri_NoParts_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, UriHelper.CombineUri());
        }

        [TestMethod]
        public void CombineUri_TrimsSlashesBetweenParts()
        {
            Assert.AreEqual("http://base/api/catalog", UriHelper.CombineUri("http://base/", "/api/catalog/"));
        }

        [TestMethod]
        public void CombineUri_HandlesNullsAndMissingParts()
        {
            Assert.AreEqual(string.Empty, UriHelper.CombineUri(null!));
            Assert.AreEqual("http://base", UriHelper.CombineUri("http://base/", null!));
        }

        [TestMethod]
        public void CombineUri_MultipleParts_JoinsWithSingleSlash()
        {
            Assert.AreEqual("a/b/c", UriHelper.CombineUri("a/", "/b/", "/c"));
        }
    }
}
