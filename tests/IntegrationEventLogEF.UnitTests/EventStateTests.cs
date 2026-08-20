using eShop.IntegrationEventLogEF;

namespace IntegrationEventLogEF.UnitTests;

[TestClass]
public class EventStateTests
{
    [TestMethod]
    [DataRow(EventState.NotPublished, 0)]
    [DataRow(EventState.InProgress, 1)]
    [DataRow(EventState.Published, 2)]
    [DataRow(EventState.PublishedFailed, 3)]
    public void Member_values_match_the_values_persisted_to_the_database(EventState state, int expectedValue)
    {
        Assert.AreEqual(expectedValue, (int)state);
    }
}
