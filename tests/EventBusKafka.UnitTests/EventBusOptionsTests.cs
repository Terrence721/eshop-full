using eShop.EventBusKafka;

namespace eShop.EventBusKafka.UnitTests
{
    [TestClass]
    public class EventBusOptionsTests
    {
        [TestMethod]
        public void RetryCount_defaults_to_ten()
        {
            var options = new EventBusOptions { SubscriptionClientName = "test" };

            Assert.AreEqual(10, options.RetryCount);
        }

        [TestMethod]
        public void RetryCount_can_be_customized()
        {
            var options = new EventBusOptions
            {
                SubscriptionClientName = "test",
                RetryCount = 5
            };

            Assert.AreEqual(5, options.RetryCount);
        }

        [TestMethod]
        public void SubscriptionClientName_can_be_set_to_null_after_initialization()
        {
            var options = new EventBusOptions { SubscriptionClientName = "test" };
            // Setting to null after required initialization is allowed (required only enforces initialization)
            options.SubscriptionClientName = null!;
            Assert.IsNull(options.SubscriptionClientName);
        }

    }
}