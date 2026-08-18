using eShop.EventBus.Abstractions;
using eShop.EventBusRabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventBusRabbitMQ.UnitTests;

[TestClass]
public class RabbitMqDependencyInjectionExtensionsTests
{
    private static HostApplicationBuilder CreateBuilder()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(
        [
            new("ConnectionStrings:rabbitmq", "amqp://localhost"),
        ]);
        return builder;
    }

    [TestMethod]
    public void AddRabbitMqEventBus_throws_ArgumentNullException_when_builder_is_null()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => RabbitMqDependencyInjectionExtensions.AddRabbitMqEventBus(null!, "rabbitmq"));
    }

    [TestMethod]
    public void AddRabbitMqEventBus_returns_a_builder_wrapping_the_same_service_collection()
    {
        var builder = CreateBuilder();

        var eventBusBuilder = builder.AddRabbitMqEventBus("rabbitmq");

        Assert.AreSame(builder.Services, eventBusBuilder.Services);
    }

    [TestMethod]
    public void AddRabbitMqEventBus_resolves_IEventBus_as_a_ResilientEventBusDecorator()
    {
        var builder = CreateBuilder();
        builder.AddRabbitMqEventBus("rabbitmq");

        using var provider = builder.Services.BuildServiceProvider();

        Assert.IsInstanceOfType<ResilientEventBusDecorator>(provider.GetRequiredService<IEventBus>());
    }

    [TestMethod]
    public void AddRabbitMqEventBus_registers_the_same_RabbitMQEventBus_singleton_as_the_IHostedService()
    {
        // The registered IHostedService (started at app startup) and the RabbitMQEventBus resolved
        // directly here must be the exact same instance -- if they weren't, StartAsync would open a
        // connection/channel on one instance while PublishAsync's decorator chain published through
        // a different, never-started instance. See the source comment this test locks in.
        var builder = CreateBuilder();
        builder.AddRabbitMqEventBus("rabbitmq");

        using var provider = builder.Services.BuildServiceProvider();

        var hostedService = provider.GetRequiredService<IHostedService>();
        var directlyResolved = provider.GetRequiredService<RabbitMQEventBus>();

        Assert.AreSame(directlyResolved, hostedService);
    }
}
