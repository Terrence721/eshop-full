using eShop.EventBus.Abstractions;
using eShop.EventBusKafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace eShop.EventBusKafka.UnitTests;

[TestClass]
public class KafkaDependencyInjectionExtensionsTests
{
    private static HostApplicationBuilder CreateBuilder()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(
        [
            new("ConnectionStrings:eventbus", "localhost:9092"),
            new("EventBus:SubscriptionClientName", "test"),
        ]);
        return builder;
    }

    [TestMethod]
    public void AddKafkaEventBus_throws_ArgumentNullException_when_builder_is_null()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => KafkaDependencyInjectionExtensions.AddKafkaEventBus(null!, "eventbus"));
    }

    [TestMethod]
    public void AddKafkaEventBus_returns_a_builder_wrapping_the_same_service_collection()
    {
        var builder = CreateBuilder();

        var eventBusBuilder = builder.AddKafkaEventBus("eventbus");

        Assert.AreSame(builder.Services, eventBusBuilder.Services);
    }

    [TestMethod]
    public void AddKafkaEventBus_resolves_IEventBus_as_a_ResilientEventBusDecorator()
    {
        var builder = CreateBuilder();
        builder.AddKafkaEventBus("eventbus");

        using var provider = builder.Services.BuildServiceProvider();

        Assert.IsInstanceOfType<ResilientEventBusDecorator>(provider.GetRequiredService<IEventBus>());
    }

    [TestMethod]
    public void AddKafkaEventBus_registers_the_same_KafkaEventBus_singleton_as_the_IHostedService()
    {
        // The registered IHostedService (started at app startup) and the KafkaEventBus resolved
        // directly here must be the exact same instance -- if they weren't, StartAsync would drive
        // the consume loop on one instance while PublishAsync's decorator chain published through
        // a different, never-started instance. See the source comment this test locks in.
        var builder = CreateBuilder();
        builder.AddKafkaEventBus("eventbus");

        using var provider = builder.Services.BuildServiceProvider();

        var hostedService = provider.GetRequiredService<IHostedService>();
        var directlyResolved = provider.GetRequiredService<KafkaEventBus>();

        Assert.AreSame(directlyResolved, hostedService);
    }
}
