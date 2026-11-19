using Microsoft.Extensions.Logging;
using NSubstitute;
using Webhooks.API.IntegrationEvents;
using Webhooks.API.Model;
using Webhooks.API.Services;

namespace Webhooks.API.UnitTests
{
    [TestClass]
    public class ProductPriceChangedIntegrationEventHandlerTests
    {
        [TestMethod]
        public async Task Handle_sends_the_CatalogItemPriceChange_subscriptions_a_webhook_built_from_the_event()
        {
            var subscriptions = new[] { new WebhookSubscription { DestUrl = "https://example.com/hook", UserId = "user-1" } };
            var retriever = Substitute.For<IWebhooksRetriever>();
            retriever.GetSubscriptionsOfType(WebhookType.CatalogItemPriceChange).Returns(subscriptions);
            var sender = Substitute.For<IWebhooksSender>();
            var handler = new ProductPriceChangedIntegrationEventHandler(
                retriever, sender, Substitute.For<ILogger<ProductPriceChangedIntegrationEventHandler>>());
            var @event = new ProductPriceChangedIntegrationEvent(1, 15.0m, 10.0m);

            await handler.Handle(@event);

            await sender.Received(1).SendAll(subscriptions, Arg.Is<WebhookData>(d => d.Type == "CatalogItemPriceChange"));
        }

        [TestMethod]
        public async Task Handle_calls_sender_even_when_there_are_no_subscriptions()
        {
            var retriever = Substitute.For<IWebhooksRetriever>();
            retriever.GetSubscriptionsOfType(WebhookType.CatalogItemPriceChange).Returns([]);
            var sender = Substitute.For<IWebhooksSender>();
            var handler = new ProductPriceChangedIntegrationEventHandler(
                retriever, sender, Substitute.For<ILogger<ProductPriceChangedIntegrationEventHandler>>());
            var @event = new ProductPriceChangedIntegrationEvent(1, 15.0m, 10.0m);

            await handler.Handle(@event);

            await sender.Received(1).SendAll(Arg.Is<IEnumerable<WebhookSubscription>>(s => !s.Any()), Arg.Any<WebhookData>());
        }

        [TestMethod]
        public async Task Handle_logs_the_number_of_subscriptions_processed()
        {
            var subscriptions = new[]
            {
                new WebhookSubscription { DestUrl = "https://a.example.com/hook", UserId = "user-1" },
                new WebhookSubscription { DestUrl = "https://b.example.com/hook", UserId = "user-2" },
            };
            var retriever = Substitute.For<IWebhooksRetriever>();
            retriever.GetSubscriptionsOfType(WebhookType.CatalogItemPriceChange).Returns(subscriptions);
            var sender = Substitute.For<IWebhooksSender>();
            var logger = Substitute.For<ILogger<ProductPriceChangedIntegrationEventHandler>>();
            var handler = new ProductPriceChangedIntegrationEventHandler(retriever, sender, logger);
            var @event = new ProductPriceChangedIntegrationEvent(1, 15.0m, 10.0m);

            await handler.Handle(@event);

            logger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("2 subscriptions")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }
    }
}