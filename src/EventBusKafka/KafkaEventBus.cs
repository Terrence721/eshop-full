namespace eShop.EventBusKafka;

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

public sealed class KafkaEventBus(
    ILogger<KafkaEventBus> logger,
    IServiceProvider serviceProvider,
    IOptions<EventBusOptions> options,
    IOptions<EventBusSubscriptionInfo> subscriptionOptions,
    KafkaTelemetry kafkaTelemetry,
    IEventSerializer eventSerializer,
    IProducer<string, byte[]> producer,
    IConsumer<string, byte[]> consumer) : IEventBus, IDisposable, IHostedService
{
    private readonly TextMapPropagator _propagator = kafkaTelemetry.Propagator;
    private readonly ActivitySource _activitySource = kafkaTelemetry.ActivitySource;
    private readonly string _consumerGroup = options.Value.SubscriptionClientName;
    private readonly EventBusSubscriptionInfo _subscriptionInfo = subscriptionOptions.Value;
    private readonly CancellationTokenSource _stoppingCts = new();
    private bool _isConsuming;

    public async Task PublishAsync(IntegrationEvent @event)
    {
        var topicName = @event.GetType().Name;

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Publishing event to Kafka: {EventId} ({EventName})", @event.Id, topicName);
        }

        var body = eventSerializer.Serialize(@event);
        var headers = new Headers();

        static void InjectTraceContextIntoHeaders(Headers headers, string key, string value)
        {
            headers.Add(key, Encoding.UTF8.GetBytes(value));
        }

        // Propagate the ambient trace context (started by TelemetryEventBusDecorator, if present)
        // into the outgoing message headers so a consumer can continue the same trace.
        var contextToInject = Activity.Current?.Context ?? default;
        _propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), headers, InjectTraceContextIntoHeaders);

        var message = new Message<string, byte[]>
        {
            Value = body,
            Headers = headers
        };

        await producer.ProduceAsync(topicName, message);
    }

    public void Dispose()
    {
        _stoppingCts.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var topics = _subscriptionInfo.EventTypes.Keys.ToList();

        if (topics.Count == 0)
        {
            // Pure publishers (e.g. OrderProcessor has zero .AddSubscription<>() calls) have
            // nothing to subscribe to -- don't start a consume loop that would never receive
            // anything.
            return Task.CompletedTask;
        }

        _isConsuming = true;
        consumer.Subscribe(topics);

        // Messaging is async so we don't need to wait for it to complete.
        _ = Task.Factory.StartNew(() => ConsumeLoop(_stoppingCts.Token), TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    private async Task ConsumeLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ConsumeResult<string, byte[]>? result;

            try
            {
                result = consumer.Consume(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error consuming from Kafka");

                // Retrying Consume() again immediately, in a tight loop, is a real anti-pattern
                // regardless of the specific error -- live-verified that it's not just wasteful:
                // it crashed the whole process with a native access violation in librdkafka
                // (0xC0000005 in rd_kafka_consumer_poll) rather than staying a managed exception.
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                continue;
            }

            if (result is null || result.IsPartitionEOF)
            {
                continue;
            }

            await OnMessageReceived(result);

            // Even on exception we take the message off the partition.
            // In a REAL WORLD app this should be handled with a dead-letter topic.
            // Matches RabbitMQEventBus's own always-ack-regardless-of-handler-exception
            // semantics exactly -- see OnMessageReceived's catch block below.
            consumer.Commit(result);
        }
    }

    private async Task OnMessageReceived(ConsumeResult<string, byte[]> result)
    {
        static IEnumerable<string> ExtractTraceContextFromHeaders(Headers headers, string key)
        {
            if (headers.TryGetLastBytes(key, out var bytes))
            {
                return [Encoding.UTF8.GetString(bytes)];
            }
            return [];
        }

        // Extract the PropagationContext of the upstream parent from the message headers.
        var parentContext = _propagator.Extract(default, result.Message.Headers, ExtractTraceContextFromHeaders);
        Baggage.Current = parentContext.Baggage;

        var activityName = $"{result.Topic} process";

        using var activity = _activitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext);

        KafkaTelemetry.SetActivityContext(activity, result.Topic, "process", _consumerGroup);

        var eventName = result.Topic;
        var message = Encoding.UTF8.GetString(result.Message.Value);

        try
        {
            activity?.SetTag("message", message);

            if (message.Contains("throw-fake-exception", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
            }

            await ProcessEvent(eventName, message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error Processing message \"{Message}\"", message);

            activity.SetExceptionTags(ex);
        }
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Processing Kafka event: {EventName}", eventName);
        }

        await using var scope = serviceProvider.CreateAsyncScope();

        if (!_subscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
            return;
        }

        // Deserialize the event
        var integrationEvent = eventSerializer.Deserialize(message, eventType);

        if (integrationEvent is null)
        {
            logger.LogWarning("Unable to deserialize event {EventName} from message", eventName);
            return;
        }

        // REVIEW: This could be done in parallel

        // Get all the handlers using the event type as the key
        foreach (var handler in scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType))
        {
            await handler.Handle(integrationEvent);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _stoppingCts.Cancel();

        // Closing the consumer (rather than just abandoning it, which RabbitMQEventBus's own
        // no-op StopAsync effectively does for its channel) commits final offsets and leaves the
        // consumer group cleanly, so the next startup doesn't wait out a rebalance timeout.
        if (_isConsuming)
        {
            consumer.Close();
        }

        return Task.CompletedTask;
    }
}
