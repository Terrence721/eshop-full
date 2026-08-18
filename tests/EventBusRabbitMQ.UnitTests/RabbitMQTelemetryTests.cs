using System.Diagnostics;
using eShop.EventBusRabbitMQ;

namespace EventBusRabbitMQ.UnitTests;

[TestClass]
public class RabbitMQTelemetryTests
{
    [TestMethod]
    public void ActivitySource_uses_the_ActivitySourceName_constant()
    {
        var telemetry = new RabbitMQTelemetry();

        Assert.AreEqual(RabbitMQTelemetry.ActivitySourceName, telemetry.ActivitySource.Name);
    }

    [TestMethod]
    public void Propagator_is_populated()
    {
        var telemetry = new RabbitMQTelemetry();

        Assert.IsNotNull(telemetry.Propagator);
    }

    [TestMethod]
    public void SetActivityContext_does_not_throw_when_activity_is_null()
    {
        RabbitMQTelemetry.SetActivityContext(null, "routing.key", "publish");
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

        RabbitMQTelemetry.SetActivityContext(activity, "my.routing.key", "publish");

        Assert.AreEqual("rabbitmq", activity!.GetTagItem("messaging.system"));
        Assert.AreEqual("queue", activity.GetTagItem("messaging.destination_kind"));
        Assert.AreEqual("publish", activity.GetTagItem("messaging.operation"));
        Assert.AreEqual("my.routing.key", activity.GetTagItem("messaging.destination.name"));
        Assert.AreEqual("my.routing.key", activity.GetTagItem("messaging.rabbitmq.routing_key"));
    }
}
