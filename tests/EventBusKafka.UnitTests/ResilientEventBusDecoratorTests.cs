using Confluent.Kafka;
using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.EventBusKafka;
using Microsoft.Extensions.Options;

namespace eShop.EventBusKafka.UnitTests;

[TestClass]
public class ResilientEventBusDecoratorTests
{
    private sealed record TestEvent : IntegrationEvent;

    private sealed class FlakyEventBus : IEventBus
    {
        private readonly Queue<Exception> _exceptionsToThrow;

        public FlakyEventBus(params Exception[] exceptionsToThrow) => _exceptionsToThrow = new(exceptionsToThrow);

        public int CallCount { get; private set; }

        public Task PublishAsync(IntegrationEvent @event)
        {
            CallCount++;

            if (_exceptionsToThrow.TryDequeue(out var exception))
            {
                throw exception;
            }

            return Task.CompletedTask;
        }
    }

    private static ResilientEventBusDecorator CreateDecorator(IEventBus inner, int retryCount) =>
        new(inner, Options.Create(new EventBusOptions { SubscriptionClientName = "test", RetryCount = retryCount }));

    [TestMethod]
    public async Task PublishAsync_succeeds_without_retrying_when_inner_succeeds_immediately()
    {
        var inner = new FlakyEventBus();
        var decorator = CreateDecorator(inner, retryCount: 3);

        await decorator.PublishAsync(new TestEvent());

        Assert.AreEqual(1, inner.CallCount);
    }

    [TestMethod]
    public async Task PublishAsync_does_not_retry_exceptions_outside_the_handled_set()
    {
        var inner = new FlakyEventBus(new InvalidOperationException("not retryable"));
        var decorator = CreateDecorator(inner, retryCount: 3);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => decorator.PublishAsync(new TestEvent()));

        Assert.AreEqual(1, inner.CallCount);
    }

    [TestMethod]
    public async Task PublishAsync_retries_and_succeeds_after_a_transient_KafkaRetriableException()
    {
        // KafkaRetriableException is Confluent.Kafka's own purpose-built "this error is safe to
        // retry" exception type (confirmed via reflection on the installed package) -- a single,
        // precise type rather than RabbitMQ's BrokerUnreachableException/SocketException pairing,
        // since the Kafka client already classifies retriable-vs-fatal errors internally.
        var inner = new FlakyEventBus(new KafkaRetriableException(new Error(ErrorCode.Local_Transport)));
        var decorator = CreateDecorator(inner, retryCount: 1);

        await decorator.PublishAsync(new TestEvent());

        Assert.AreEqual(2, inner.CallCount);
    }
}
