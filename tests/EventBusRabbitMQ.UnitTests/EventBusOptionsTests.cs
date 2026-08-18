using eShop.EventBusRabbitMQ;

namespace EventBusRabbitMQ.UnitTests;

[TestClass]
public class EventBusOptionsTests
{
    [TestMethod]
    public void RetryCount_defaults_to_ten()
    {
        var options = new EventBusOptions { SubscriptionClientName = "test" };

        Assert.AreEqual(10, options.RetryCount);
    }
}
