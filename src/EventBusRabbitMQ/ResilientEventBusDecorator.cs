namespace eShop.EventBusRabbitMQ;

using Microsoft.Extensions.Options;
using Polly.Retry;

public sealed class ResilientEventBusDecorator : IEventBus
{
    private readonly IEventBus _inner;
    private readonly ResiliencePipeline _pipeline;

    public ResilientEventBusDecorator(IEventBus inner, IOptions<EventBusOptions> options)
    {
        _inner = inner;
        _pipeline = CreateResiliencePipeline(options.Value.RetryCount);
    }

    public Task PublishAsync(IntegrationEvent @event) =>
        _pipeline.ExecuteAsync(
            static (state, cancellationToken) => new ValueTask(state.inner.PublishAsync(state.@event)),
            (inner: _inner, @event),
            CancellationToken.None).AsTask();

    private static ResiliencePipeline CreateResiliencePipeline(int retryCount)
    {
        // See https://www.pollydocs.org/strategies/retry.html
        var retryOptions = new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<BrokerUnreachableException>().Handle<SocketException>(),
            MaxRetryAttempts = retryCount,
            DelayGenerator = (context) => ValueTask.FromResult(GenerateDelay(context.AttemptNumber))
        };

        return new ResiliencePipelineBuilder()
            .AddRetry(retryOptions)
            .Build();

        static TimeSpan? GenerateDelay(int attempt)
        {
            return TimeSpan.FromSeconds(Math.Pow(2, attempt));
        }
    }
}
