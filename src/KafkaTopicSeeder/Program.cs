using Confluent.Kafka;
using Confluent.Kafka.Admin;

var bootstrapServers = Environment.GetEnvironmentVariable("ConnectionStrings__eventbus")
    ?? throw new InvalidOperationException("Missing environment variable 'ConnectionStrings__eventbus' -- this project is meant to run under eShop.AppHost, referencing the eventbus Kafka resource.");

// One topic per IntegrationEvent type name, matching KafkaEventBus.PublishAsync's own
// @event.GetType().Name topic naming exactly. Listed here rather than reflected from every
// consuming project's assembly, since that would mean this project referencing all 7 of them
// for what's otherwise a plain list of strings -- confirmed complete against every real
// AddSubscription<T,...>() call in the solution, not guessed.
string[] topicNames =
[
    "GracePeriodConfirmedIntegrationEvent",
    "OrderPaymentFailedIntegrationEvent",
    "OrderPaymentSucceededIntegrationEvent",
    "OrderStartedIntegrationEvent",
    "OrderStatusChangedToAwaitingValidationIntegrationEvent",
    "OrderStatusChangedToCancelledIntegrationEvent",
    "OrderStatusChangedToPaidIntegrationEvent",
    "OrderStatusChangedToShippedIntegrationEvent",
    "OrderStatusChangedToStockConfirmedIntegrationEvent",
    "OrderStatusChangedToSubmittedIntegrationEvent",
    "OrderStockConfirmedIntegrationEvent",
    "OrderStockRejectedIntegrationEvent",
    "ProductPriceChangedIntegrationEvent",
];

using var adminClient = new AdminClientBuilder(new AdminClientConfig
{
    BootstrapServers = bootstrapServers,
    BrokerAddressFamily = BrokerAddressFamily.V4,
}).Build();

var specs = topicNames
    .Select(name => new TopicSpecification { Name = name, NumPartitions = 3, ReplicationFactor = 1 })
    .ToList();

try
{
    await adminClient.CreateTopicsAsync(specs, new CreateTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(30) });
    Console.WriteLine($"Created {specs.Count} Kafka topics.");
}
catch (CreateTopicsException ex)
{
    // Results lists every topic in the request, success and failure alike -- CreateTopicsAsync
    // throws if even one has a real error, so this has to filter to the genuine failures itself,
    // not just assume everything in Results failed. Re-running this seeder against a broker that
    // already has these topics from a prior AppHost session is expected too, not an error.
    var realFailures = ex.Results.Where(r => r.Error.IsError && r.Error.Code != ErrorCode.TopicAlreadyExists).ToList();
    if (realFailures.Count > 0)
    {
        throw new InvalidOperationException(
            $"Failed to create Kafka topics: {string.Join(", ", realFailures.Select(r => $"{r.Topic}: {r.Error.Reason}"))}");
    }

    Console.WriteLine($"{topicNames.Length} Kafka topics already existed.");
}
