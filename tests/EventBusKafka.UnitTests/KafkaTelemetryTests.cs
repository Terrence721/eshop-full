using System.Diagnostics;
using eShop.EventBusKafka;

namespace eShop.EventBusKafka.UnitTests
{
    [TestClass]
    public class KafkaTelemetryTests
    {
        [TestMethod]
        public void ActivitySource_uses_the_ActivitySourceName_constant()
        {
            var telemetry = new KafkaTelemetry();

            Assert.AreEqual(KafkaTelemetry.ActivitySourceName, telemetry.ActivitySource.Name);
        }

        [TestMethod]
        public void Propagator_is_populated()
        {
            var telemetry = new KafkaTelemetry();

            Assert.IsNotNull(telemetry.Propagator);
        }

        [TestMethod]
        public void SetActivityContext_does_not_throw_when_activity_is_null()
        {
            KafkaTelemetry.SetActivityContext(null, "my-topic", "send");
        }

        [TestMethod]
        public void SetActivityContext_sets_the_messaging_semantic_convention_tags()
        {
            using var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            };
            ActivitySource.AddActivityListener(listener);
            using var source = new ActivitySource(nameof(SetActivityContext_sets_the_messaging_semantic_convention_tags));
            using var activity = source.StartActivity("test-activity");

            KafkaTelemetry.SetActivityContext(activity, "my-topic", "process", "my-consumer-group");

            Assert.AreEqual("kafka", activity!.GetTagItem("messaging.system"));
            Assert.AreEqual("my-topic", activity.GetTagItem("messaging.destination.name"));
            Assert.AreEqual("process", activity.GetTagItem("messaging.operation.name"));
            Assert.AreEqual("my-consumer-group", activity.GetTagItem("messaging.consumer.group.name"));
        }

        [TestMethod]
        public void SetActivityContext_omits_the_consumer_group_tag_when_not_given()
        {
            // Per the current OpenTelemetry Kafka semantic conventions, a consumer group is only
            // meaningful on the consume side -- a publish span has no consumer group, so the publish
            // call site (TelemetryEventBusDecorator) simply omits it rather than passing null/empty.
            using var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            };
            ActivitySource.AddActivityListener(listener);
            using var source = new ActivitySource(nameof(SetActivityContext_omits_the_consumer_group_tag_when_not_given));
            using var activity = source.StartActivity("test-activity");

            KafkaTelemetry.SetActivityContext(activity, "my-topic", "send");

            Assert.IsNull(activity!.GetTagItem("messaging.consumer.group.name"));
        }
    }
}
