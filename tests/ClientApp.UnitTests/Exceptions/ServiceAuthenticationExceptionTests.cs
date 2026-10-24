using eShop.ClientApp.Exceptions;

namespace eShop.ClientApp.UnitTests.Exceptions
{
    [TestClass]
    public class ServiceAuthenticationExceptionTests
    {
        [TestMethod]
        public void DefaultConstructor_SetsNullContent()
        {
            var ex = new ServiceAuthenticationException();

            Assert.IsNull(ex.Content);
        }

        [TestMethod]
        public void ContentConstructor_SetsContent()
        {
            const string content = "unauthorized response body";

            var ex = new ServiceAuthenticationException(content);

            Assert.AreEqual(content, ex.Content);
        }

        [TestMethod]
        public void ContentConstructor_MessageDefaultsToEmpty()
        {
            var ex = new ServiceAuthenticationException("payload");

            Assert.IsNotNull(ex.Message);
        }

        [TestMethod]
        public void IsException()
        {
            var ex = new ServiceAuthenticationException("payload");

            Assert.IsInstanceOfType<Exception>(ex);
        }
    }
}
