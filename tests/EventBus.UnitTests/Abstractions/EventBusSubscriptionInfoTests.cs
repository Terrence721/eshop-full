using eShop.EventBus.Abstractions;

namespace EventBus.UnitTests.Abstractions;

[TestClass]
public class EventBusSubscriptionInfoTests
{
    [TestMethod]
    public void EventTypes_starts_empty()
    {
        var info = new EventBusSubscriptionInfo();

        Assert.AreEqual(0, info.EventTypes.Count);
    }

    [TestMethod]
    public void JsonSerializerOptions_is_populated_with_a_TypeInfoResolver()
    {
        var info = new EventBusSubscriptionInfo();

        Assert.IsNotNull(info.JsonSerializerOptions.TypeInfoResolver);
    }

    [TestMethod]
    public void JsonSerializerOptions_instances_are_independent_across_EventBusSubscriptionInfo_instances()
    {
        var first = new EventBusSubscriptionInfo();
        var second = new EventBusSubscriptionInfo();

        Assert.AreNotSame(first.JsonSerializerOptions, second.JsonSerializerOptions);

        first.JsonSerializerOptions.WriteIndented = true;

        Assert.IsFalse(second.JsonSerializerOptions.WriteIndented);
    }
}
