using eShop.WebBFF.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.WebBFF.UnitTests
{
    [TestClass]
    public class ClaudeChatServiceTests
    {
        private static ClaudeChatService CreateService(string? apiKey = "mock-api-key")
        {
            var factory = Substitute.For<IHttpClientFactory>();
            var settings = new AppSettings(apiKey);
            var logger = Substitute.For<ILogger<ClaudeChatService>>();
            return new ClaudeChatService(factory, settings, logger);
        }

        [TestMethod]
        public async Task SendUserMessageAsync_throws_when_api_key_is_not_configured()
        {
            var service = CreateService(apiKey: null!);
            InvalidOperationException? caught = null;
            try
            {
                await service.SendUserMessageAsync("buyer-1", "Hello");
            }
            catch (InvalidOperationException ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught);
            Assert.IsTrue(caught!.Message.Contains("AnthropicApiKey not configured"));
        }

        [TestMethod]
        public async Task SendUserMessageAsync_throws_when_network_request_fails()
        {
            var handler = new HttpMessageHandlerWrapper(() => throw new HttpRequestException("network failure"));
            var factory = Substitute.For<IHttpClientFactory>();
            factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));
            var settings = new AppSettings("mock-api-key");
            var logger = Substitute.For<ILogger<ClaudeChatService>>();
            var service = new ClaudeChatService(factory, settings, logger);

            HttpRequestException? caught = null;
            try
            {
                await service.SendUserMessageAsync("buyer-1", "Hello");
            }
            catch (HttpRequestException ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught);
            Assert.AreEqual("network failure", caught!.Message);
        }

        [TestMethod]
        public void ClearConversation_no_op_for_unknown_buyer()
        {
            var service = CreateService();

            // Should not throw for an unknown buyer
            service.ClearConversation("unknown-buyer");
        }

        [TestMethod]
        public void ClearConversation_does_not_call_the_http_client_for_an_unknown_buyer()
        {
            var handler = new HttpMessageHandlerWrapper(() => throw new HttpRequestException("network failure"));
            var factory = Substitute.For<IHttpClientFactory>();
            factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));
            var settings = new AppSettings("mock-api-key");
            var logger = Substitute.For<ILogger<ClaudeChatService>>();
            var service = new ClaudeChatService(factory, settings, logger);

            // Should not make HTTP call for unknown buyer
            service.ClearConversation("unknown-buyer");
            factory.DidNotReceive().CreateClient(Arg.Any<string>());
        }

        [TestMethod]
        public void HasOngoingStreaming_always_returns_false()
        {
            var service = CreateService();

            Assert.IsFalse(service.HasOngoingStreaming);
        }

        [TestMethod]
        public async Task StreamAssistantResponseAsync_throws_when_api_key_not_configured()
        {
            var service = CreateService(apiKey: null!);
            InvalidOperationException? caught = null;
            try
            {
                await foreach (var _ in service.StreamAssistantResponseAsync("buyer-1", "Hi", CancellationToken.None)) { }
            }
            catch (InvalidOperationException ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught);
            Assert.IsTrue(caught.Message.Contains("AnthropicApiKey not configured"));
        }

        [TestMethod]
        public async Task StreamAssistantResponseAsync_throws_when_network_request_fails()
        {
            var handler = new HttpMessageHandlerWrapper(() => throw new HttpRequestException("network failure"));
            var factory = Substitute.For<IHttpClientFactory>();
            factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));
            var settings = new AppSettings("mock-api-key");
            var logger = Substitute.For<ILogger<ClaudeChatService>>();
            var service = new ClaudeChatService(factory, settings, logger);

            HttpRequestException? caught = null;
            try
            {
                await foreach (var _ in service.StreamAssistantResponseAsync("buyer-1", "Hi", CancellationToken.None)) { }
            }
            catch (HttpRequestException ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught);
            Assert.AreEqual("network failure", caught!.Message);
        }
    }

    // Helper for testing network failures
    internal sealed class HttpMessageHandlerWrapper : HttpMessageHandler
    {
        private readonly Func<Exception> _thrower;

        public HttpMessageHandlerWrapper(Func<Exception> thrower)
        {
            _thrower = thrower;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw _thrower();
    }
}
