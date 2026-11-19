using System.Text.Json;
using eShop.EventBus.Abstractions;

namespace eShop.EventBus.UnitTests.Abstractions
{
    [TestClass]
    public class EventBusSubscriptionInfoTests
    {
        [TestMethod]
        public void EventTypes_dictionary_is_initially_empty()
        {
            var info = new EventBusSubscriptionInfo();

            Assert.AreEqual(0, info.EventTypes.Count);
        }

        [TestMethod]
        public void EventTypes_can_add_and_retrieve_event_types()
        {
            var info = new EventBusSubscriptionInfo();
            var testEventType = typeof(TestEvent);

            info.EventTypes[testEventType.Name] = testEventType;

            Assert.AreEqual(testEventType, info.EventTypes[testEventType.Name]);
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

        [TestMethod]
        public void Default_options_serialize_and_deserialize_a_plain_object_round_trip()
        {
            var info = new EventBusSubscriptionInfo();
            var payload = new { Id = 42, Name = "test" };

            var json = JsonSerializer.Serialize(payload, info.JsonSerializerOptions);
            var deserialized = JsonSerializer.Deserialize<JsonElement>(json, info.JsonSerializerOptions);

            Assert.AreEqual(42, deserialized.GetProperty("Id").GetInt32());
            Assert.AreEqual("test", deserialized.GetProperty("Name").GetString());
        }

        private sealed record TestEvent;
    }
}