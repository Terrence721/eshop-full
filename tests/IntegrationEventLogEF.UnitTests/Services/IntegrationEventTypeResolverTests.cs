using eShop.EventBus.Events;
using eShop.IntegrationEventLogEF.Services;

namespace eShop.IntegrationEventLogEF.UnitTests.Services
{
    [TestClass]
    public class IntegrationEventTypeResolverTests
    {
        private sealed record SomeIntegrationEvent : IntegrationEvent
        {
            public string? Payload { get; init; }
        }

        private sealed record AnotherIntegrationEvent : IntegrationEvent
        {
            public string? Payload { get; init; }
        }

        [TestMethod]
        public void BuildEventTypesByShortName_indexes_candidate_types_by_their_simple_name()
        {
            Type[] candidates = [typeof(SomeIntegrationEvent), typeof(AnotherIntegrationEvent)];

            var result = IntegrationEventTypeResolver.BuildEventTypesByShortName(candidates);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(typeof(SomeIntegrationEvent), result[typeof(SomeIntegrationEvent).Name]);
            Assert.AreEqual(typeof(AnotherIntegrationEvent), result[typeof(AnotherIntegrationEvent).Name]);
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
            Type[] candidates = [typeof(SomeIntegrationEvent), typeof(SomeIntegrationEvent)];

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => IntegrationEventTypeResolver.BuildEventTypesByShortName(candidates));

            StringAssert.Contains(exception.Message, "SomeIntegrationEvent");
        }

        [TestMethod]
        public void Resolve_returns_the_matching_type()
        {
            var eventTypesByShortName = IntegrationEventTypeResolver.BuildEventTypesByShortName([typeof(SomeIntegrationEvent)]);

            var result = IntegrationEventTypeResolver.Resolve(eventTypesByShortName, nameof(SomeIntegrationEvent));

            Assert.AreEqual(typeof(SomeIntegrationEvent), result);
        }

        [TestMethod]
        public void Resolve_throws_when_no_type_matches_the_short_name()
        {
            var eventTypesByShortName = IntegrationEventTypeResolver.BuildEventTypesByShortName([typeof(SomeIntegrationEvent)]);

            var exception = Assert.ThrowsExactly<InvalidOperationException>(
                () => IntegrationEventTypeResolver.Resolve(eventTypesByShortName, "NonexistentIntegrationEvent"));

            StringAssert.Contains(exception.Message, "NonexistentIntegrationEvent");
        }
    }
}