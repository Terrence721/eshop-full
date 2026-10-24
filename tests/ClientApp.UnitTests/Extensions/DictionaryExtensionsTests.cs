using eShop.ClientApp;

namespace eShop.ClientApp.UnitTests.Extensions
{
    [TestClass]
    public class DictionaryExtensionsTests
    {
        [TestMethod]
        public void ValueAsBool_ExistingBool_ReturnsValue()
        {
            var dict = new Dictionary<string, object> { ["flag"] = true };

            Assert.IsTrue(dict.ValueAsBool("flag"));
        }

        [TestMethod]
        public void ValueAsBool_MissingOrWrongType_ReturnsDefault()
        {
            var dict = new Dictionary<string, object> { ["flag"] = "true" };

            Assert.IsFalse(dict.ValueAsBool("flag"));
            Assert.IsFalse(dict.ValueAsBool("missing"));
            Assert.IsTrue(dict.ValueAsBool("missing", true));
        }

        [TestMethod]
        public void ValueAsInt_ExistingInt_ReturnsValue()
        {
            var dict = new Dictionary<string, object> { ["count"] = 42 };

            Assert.AreEqual(42, dict.ValueAsInt("count"));
        }

        [TestMethod]
        public void ValueAsInt_MissingOrWrongType_ReturnsDefault()
        {
            var dict = new Dictionary<string, object> { ["count"] = "42" };

            Assert.AreEqual(0, dict.ValueAsInt("count"));
            Assert.AreEqual(0, dict.ValueAsInt("missing"));
            Assert.AreEqual(7, dict.ValueAsInt("missing", 7));
        }

        [TestMethod]
        public void ValueAs_ExistingMatchingType_ReturnsValue()
        {
            var dict = new Dictionary<string, object> { ["name"] = "Sam" };

            Assert.AreEqual("Sam", dict.ValueAs<string>("name"));
        }

        [TestMethod]
        public void ValueAs_MissingOrWrongType_ReturnsDefault()
        {
            var dict = new Dictionary<string, object> { ["name"] = 123 };

            Assert.IsNull(dict.ValueAs<string>("name"));
            Assert.AreEqual("fallback", dict.ValueAs<string>("missing", "fallback"));
        }
    }
}
