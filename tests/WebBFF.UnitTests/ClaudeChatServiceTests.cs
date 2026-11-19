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
        public void ClearConversation_no_op_for_unknown_buyer()
        {
            var service = CreateService();

            // Should not throw for an unknown buyer
            service.ClearConversation("unknown-buyer");
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
    }
}
