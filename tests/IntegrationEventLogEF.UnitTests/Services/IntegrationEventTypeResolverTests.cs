using eShop.IntegrationEventLogEF.Services;

namespace IntegrationEventLogEF.UnitTests.Services;

[TestClass]
public class IntegrationEventTypeResolverTests
{
    private sealed class OrderNamespace
    {
        public sealed class OrderPlacedIntegrationEvent;
    }

    private sealed class CatalogNamespace
    {
        public sealed class OrderPlacedIntegrationEvent;
    }

    private sealed class BasketNamespace
    {
        public sealed class BasketCheckoutIntegrationEvent;
    }

    [TestMethod]
    public void BuildEventTypesByShortName_indexes_candidate_types_by_their_simple_name()
    {
        Type[] candidates = [typeof(OrderNamespace.OrderPlacedIntegrationEvent), typeof(BasketNamespace.BasketCheckoutIntegrationEvent)];

        var result = IntegrationEventTypeResolver.BuildEventTypesByShortName(candidates);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(typeof(OrderNamespace.OrderPlacedIntegrationEvent), result[nameof(OrderNamespace.OrderPlacedIntegrationEvent)]);
        Assert.AreEqual(typeof(BasketNamespace.BasketCheckoutIntegrationEvent), result[nameof(BasketNamespace.BasketCheckoutIntegrationEvent)]);
    }

    [TestMethod]
    public void BuildEventTypesByShortName_returns_an_empty_dictionary_for_no_candidates()
    {
        var result = IntegrationEventTypeResolver.BuildEventTypesByShortName([]);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void BuildEventTypesByShortName_throws_when_two_candidates_share_a_simple_name()
    {
        Type[] candidates = [typeof(OrderNamespace.OrderPlacedIntegrationEvent), typeof(CatalogNamespace.OrderPlacedIntegrationEvent)];

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => IntegrationEventTypeResolver.BuildEventTypesByShortName(candidates));

        StringAssert.Contains(exception.Message, nameof(OrderNamespace.OrderPlacedIntegrationEvent));
    }

    [TestMethod]
    public void Resolve_returns_the_matching_type()
    {
        var eventTypesByShortName = IntegrationEventTypeResolver.BuildEventTypesByShortName([typeof(OrderNamespace.OrderPlacedIntegrationEvent)]);

        var result = IntegrationEventTypeResolver.Resolve(eventTypesByShortName, nameof(OrderNamespace.OrderPlacedIntegrationEvent));

        Assert.AreEqual(typeof(OrderNamespace.OrderPlacedIntegrationEvent), result);
    }

    [TestMethod]
    public void Resolve_throws_when_no_type_matches_the_short_name()
    {
        var eventTypesByShortName = IntegrationEventTypeResolver.BuildEventTypesByShortName([typeof(OrderNamespace.OrderPlacedIntegrationEvent)]);

        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => IntegrationEventTypeResolver.Resolve(eventTypesByShortName, "NonexistentIntegrationEvent"));

        StringAssert.Contains(exception.Message, "NonexistentIntegrationEvent");
    }
}
