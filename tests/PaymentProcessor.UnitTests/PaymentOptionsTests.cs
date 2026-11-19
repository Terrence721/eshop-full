using eShop.PaymentProcessor;

namespace eShop.PaymentProcessor.UnitTests
{
    [TestClass]
    public class PaymentOptionsTests
    {
        [TestMethod]
        public void PaymentSucceeded_defaults_to_false()
        {
            var options = new PaymentOptions();
            Assert.IsFalse(options.PaymentSucceeded);
        }

        [TestMethod]
        public void PaymentSucceeded_can_be_set_to_true()
        {
            var options = new PaymentOptions { PaymentSucceeded = true };
            Assert.IsTrue(options.PaymentSucceeded);
        }

        [TestMethod]
        public void PaymentSucceeded_can_be_set_to_false()
        {
            var options = new PaymentOptions { PaymentSucceeded = false };
            Assert.IsFalse(options.PaymentSucceeded);
        }
    }
}