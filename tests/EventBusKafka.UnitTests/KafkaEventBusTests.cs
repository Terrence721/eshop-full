using Confluent.Kafka;
using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.EventBusKafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace eShop.EventBusKafka.UnitTests;

[TestClass]
public class KafkaEventBusTests
{
    private sealed record TestEvent : IntegrationEvent;

    // Confluent.Kafka's IProducer<,> has a large surface area (transactions, raw Produce, etc.)
    // that KafkaEventBus never touches -- only the members it actually calls are implemented for
    // real; everything else throws, so a test fails loudly instead of silently if that changes.
    private sealed class FakeProducer : IProducer<string, byte[]>
    {
        public string? LastTopic { get; private set; }
        public Message<string, byte[]>? LastMessage { get; private set; }

        public Task<DeliveryResult<string, byte[]>> ProduceAsync(string topic, Message<string, byte[]> message, CancellationToken cancellationToken = default)
        {
            LastTopic = topic;
            LastMessage = message;
            return Task.FromResult(new DeliveryResult<string, byte[]> { Topic = topic, Message = message });
        }

        public Task<DeliveryResult<string, byte[]>> ProduceAsync(TopicPartition topicPartition, Message<string, byte[]> message, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void Produce(string topic, Message<string, byte[]> message, Action<DeliveryReport<string, byte[]>>? deliveryHandler = null) => throw new NotImplementedException();
        public void Produce(TopicPartition topicPartition, Message<string, byte[]> message, Action<DeliveryReport<string, byte[]>>? deliveryHandler = null) => throw new NotImplementedException();
        public int Poll(TimeSpan timeout) => throw new NotImplementedException();
        public int Flush(TimeSpan timeout) => throw new NotImplementedException();
        public void Flush(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void InitTransactions(TimeSpan timeout) => throw new NotImplementedException();
        public void BeginTransaction() => throw new NotImplementedException();
        public void CommitTransaction(TimeSpan timeout) => throw new NotImplementedException();
        public void CommitTransaction() => throw new NotImplementedException();
        public void AbortTransaction(TimeSpan timeout) => throw new NotImplementedException();
        public void AbortTransaction() => throw new NotImplementedException();
        public void SendOffsetsToTransaction(IEnumerable<TopicPartitionOffset> offsets, IConsumerGroupMetadata groupMetadata, TimeSpan timeout) => throw new NotImplementedException();
        public Handle Handle => throw new NotImplementedException();
        public string Name => "fake-producer";
        public int AddBrokers(string brokers) => throw new NotImplementedException();
        public void SetSaslCredentials(string username, string password) => throw new NotImplementedException();
        public void Dispose() { }
    }

    // Same rationale as FakeProducer above -- only Subscribe and Close are exercised by the
    // scenarios under test, so only those are real; the rest throw.
    private sealed class FakeConsumer : IConsumer<string, byte[]>
    {
        public bool SubscribeCalled { get; private set; }
        public bool CloseCalled { get; private set; }

        public void Subscribe(IEnumerable<string> topics) => SubscribeCalled = true;
        public void Close() => CloseCalled = true;

        public ConsumeResult<string, byte[]> Consume(int millisecondsTimeout) => throw new NotImplementedException();
        public ConsumeResult<string, byte[]> Consume(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public ConsumeResult<string, byte[]> Consume(TimeSpan timeout) => throw new NotImplementedException();
        public string MemberId => throw new NotImplementedException();
        public List<TopicPartition> Assignment => throw new NotImplementedException();
        public List<string> Subscription => throw new NotImplementedException();
        public void Subscribe(string topic) => throw new NotImplementedException();
        public void Unsubscribe() => throw new NotImplementedException();
        public void Assign(TopicPartition partition) => throw new NotImplementedException();
        public void Assign(TopicPartitionOffset partition) => throw new NotImplementedException();
        public void Assign(IEnumerable<TopicPartitionOffset> partitions) => throw new NotImplementedException();
        public void Assign(IEnumerable<TopicPartition> partitions) => throw new NotImplementedException();
        public void IncrementalAssign(IEnumerable<TopicPartitionOffset> partitions) => throw new NotImplementedException();
        public void IncrementalAssign(IEnumerable<TopicPartition> partitions) => throw new NotImplementedException();
        public void IncrementalUnassign(IEnumerable<TopicPartition> partitions) => throw new NotImplementedException();
        public void Unassign() => throw new NotImplementedException();
        public void StoreOffset(ConsumeResult<string, byte[]> result) => throw new NotImplementedException();
        public void StoreOffset(TopicPartitionOffset offset) => throw new NotImplementedException();
        public List<TopicPartitionOffset> Commit() => throw new NotImplementedException();
        public void Commit(IEnumerable<TopicPartitionOffset> offsets) => throw new NotImplementedException();
        public void Commit(ConsumeResult<string, byte[]> result) => throw new NotImplementedException();
        public void Seek(TopicPartitionOffset tpo) => throw new NotImplementedException();
        public void Pause(IEnumerable<TopicPartition> partitions) => throw new NotImplementedException();
        public void Resume(IEnumerable<TopicPartition> partitions) => throw new NotImplementedException();
        public List<TopicPartitionOffset> Committed(TimeSpan timeout) => throw new NotImplementedException();
        public List<TopicPartitionOffset> Committed(IEnumerable<TopicPartition> partitions, TimeSpan timeout) => throw new NotImplementedException();
        public Offset Position(TopicPartition partition) => throw new NotImplementedException();
        public List<TopicPartitionOffset> OffsetsForTimes(IEnumerable<TopicPartitionTimestamp> timestampsToSearch, TimeSpan timeout) => throw new NotImplementedException();
        public WatermarkOffsets GetWatermarkOffsets(TopicPartition topicPartition) => throw new NotImplementedException();
        public WatermarkOffsets QueryWatermarkOffsets(TopicPartition topicPartition, TimeSpan timeout) => throw new NotImplementedException();
        public IConsumerGroupMetadata ConsumerGroupMetadata => throw new NotImplementedException();
        public Handle Handle => throw new NotImplementedException();
        public string Name => "fake-consumer";
        public int AddBrokers(string brokers) => throw new NotImplementedException();
        public void SetSaslCredentials(string username, string password) => throw new NotImplementedException();
        public void Dispose() { }
    }

    private static KafkaEventBus CreateEventBus(
        FakeProducer? producer = null,
        FakeConsumer? consumer = null,
        EventBusSubscriptionInfo? subscriptionInfo = null) => new(
        NullLogger<KafkaEventBus>.Instance,
        new ServiceCollection().BuildServiceProvider(),
        Options.Create(new EventBusOptions { SubscriptionClientName = "test" }),
        Options.Create(subscriptionInfo ?? new EventBusSubscriptionInfo()),
        new KafkaTelemetry(),
        new SystemTextJsonEventSerializer(Options.Create(new EventBusSubscriptionInfo())),
        producer ?? new FakeProducer(),
        consumer ?? new FakeConsumer());

    [TestMethod]
    public async Task PublishAsync_produces_to_a_topic_named_after_the_event_type()
    {
        var producer = new FakeProducer();
        IEventBus eventBus = CreateEventBus(producer: producer);

        await eventBus.PublishAsync(new TestEvent());

        Assert.AreEqual(nameof(TestEvent), producer.LastTopic);
        Assert.IsNotNull(producer.LastMessage);
    }

    [TestMethod]
    public async Task StartAsync_does_not_subscribe_when_there_are_no_subscriptions()
    {
        // Regression test: pure publishers (e.g. OrderProcessor has zero .AddSubscription<>()
        // calls) must not start a consume loop that would never receive anything -- see the
        // source comment this test locks in.
        var consumer = new FakeConsumer();
        var eventBus = CreateEventBus(consumer: consumer, subscriptionInfo: new EventBusSubscriptionInfo());

        await eventBus.StartAsync(CancellationToken.None);

        Assert.IsFalse(consumer.SubscribeCalled);
    }

    [TestMethod]
    public async Task StopAsync_does_not_close_the_consumer_when_StartAsync_never_subscribed()
    {
        var consumer = new FakeConsumer();
        var eventBus = CreateEventBus(consumer: consumer, subscriptionInfo: new EventBusSubscriptionInfo());
        await eventBus.StartAsync(CancellationToken.None);

        await eventBus.StopAsync(CancellationToken.None);

        Assert.IsFalse(consumer.CloseCalled);
    }
}
