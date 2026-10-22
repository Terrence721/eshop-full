using System.Net;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Webhooks.API.Services;

namespace Webhooks.API.UnitTests
{
    [TestClass]
    public class GrantUrlTesterServiceTests
    {
        private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
        {
            public List<HttpRequestMessage> Requests { get; } = [];

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                return Task.FromResult(respond(request));
            }
        }

        private static (IGrantUrlTesterService service, FakeHandler handler) CreateService(Func<HttpRequestMessage, HttpResponseMessage> respond)
        {
            var handler = new FakeHandler(respond);
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));
            return (new GrantUrlTesterService(httpClientFactory, Substitute.For<ILogger<IGrantUrlTesterService>>()), handler);
        }

        private static HttpResponseMessage RespondWithToken(HttpStatusCode status, string? token)
        {
            var response = new HttpResponseMessage(status);
            if (token != null)
            {
                response.Headers.Add("X-eshop-whtoken", token);
            }
            return response;
        }

        [TestMethod]
        public async Task TestGrantUrl_returns_false_without_sending_a_request_when_origins_differ()
        {
            var (service, handler) = CreateService(_ => RespondWithToken(HttpStatusCode.OK, "tok"));

            var result = await service.TestGrantUrl("https://hooks.example.com/hook", "https://grants.example.com/grant", "tok");

            Assert.IsFalse(result);
            Assert.HasCount(0, handler.Requests);
        }

        [TestMethod]
        public async Task TestGrantUrl_returns_true_when_the_response_echoes_the_expected_token()
        {
            var (service, _) = CreateService(_ => RespondWithToken(HttpStatusCode.OK, "tok"));

            var result = await service.TestGrantUrl("https://example.com/hook", "https://example.com/grant", "tok");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestGrantUrl_returns_false_when_the_response_token_does_not_match()
        {
            var (service, _) = CreateService(_ => RespondWithToken(HttpStatusCode.OK, "wrong-token"));

            var result = await service.TestGrantUrl("https://example.com/hook", "https://example.com/grant", "tok");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TestGrantUrl_returns_false_when_the_response_is_not_successful()
        {
            var (service, _) = CreateService(_ => RespondWithToken(HttpStatusCode.NotFound, "tok"));

            var result = await service.TestGrantUrl("https://example.com/hook", "https://example.com/grant", "tok");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TestGrantUrl_returns_false_when_sending_the_request_throws()
        {
            var (service, _) = CreateService(_ => throw new HttpRequestException("boom"));

            var result = await service.TestGrantUrl("https://example.com/hook", "https://example.com/grant", "tok");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TestGrantUrl_treats_an_empty_token_as_no_token_expected()
        {
            var (service, _) = CreateService(_ => RespondWithToken(HttpStatusCode.OK, token: null));

            var result = await service.TestGrantUrl("https://example.com/hook", "https://example.com/grant", string.Empty);

            Assert.IsTrue(result);
        }
    }
}
