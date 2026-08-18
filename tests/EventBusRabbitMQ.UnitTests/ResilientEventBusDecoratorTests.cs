using System.Net.Sockets;
using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.EventBusRabbitMQ;
using Microsoft.Extensions.Options;

namespace EventBusRabbitMQ.UnitTests;

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
    public async Task PublishAsync_retries_and_succeeds_after_a_transient_SocketException()
    {
        // Regression test for the real bug already found and fixed in this class (see todo.md's
        // EventBusRabbitMQ section): the original code bound to Polly's synchronous Execute<TResult>
        // overload on an async lambda, so it never actually awaited the inner call and never observed
        // the exceptions it was configured to retry on -- the retry logic was inert. This is the first
        // time that fix has been verified end-to-end rather than only against the reflected Polly API.
        var inner = new FlakyEventBus(new SocketException());
        var decorator = CreateDecorator(inner, retryCount: 1);

        await decorator.PublishAsync(new TestEvent());

        Assert.AreEqual(2, inner.CallCount);
    }
}
