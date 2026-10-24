using eShop.ClientApp.Exceptions;
using eShop.ClientApp.Services.RequestProvider;
using System.Net;

namespace eShop.ClientApp.UnitTests.Services.RequestProvider
{
    public class FakeHandler : HttpMessageHandler
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string Content { get; set; } = string.Empty;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(StatusCode)
            {
                Content = new StringContent(Content),
            };
            return Task.FromResult(response);
        }
    }

    [TestClass]
    public class RequestProviderTests
    {
        private static eShop.ClientApp.Services.RequestProvider.RequestProvider Create(FakeHandler handler)
            => new eShop.ClientApp.Services.RequestProvider.RequestProvider(handler);

        [TestMethod]
        public async Task GetAsync_Unauthorized_ThrowsServiceAuthenticationException()
        {
            var handler = new FakeHandler { StatusCode = HttpStatusCode.Unauthorized, Content = "denied" };
            var provider = Create(handler);

            try
            {
                await provider.GetAsync<object>("http://localhost/test");
                Assert.Fail("Expected ServiceAuthenticationException");
            }
            catch (ServiceAuthenticationException ex)
            {
                Assert.AreEqual("denied", ex.Content);
            }
        }

        [TestMethod]
        public async Task GetAsync_Forbidden_ThrowsServiceAuthenticationException()
        {
            var handler = new FakeHandler { StatusCode = HttpStatusCode.Forbidden, Content = "forbidden" };
            var provider = Create(handler);

            try
            {
                await provider.GetAsync<object>("http://localhost/test");
                Assert.Fail("Expected ServiceAuthenticationException");
            }
            catch (ServiceAuthenticationException)
            {
            }
        }

        [TestMethod]
        public async Task GetAsync_BadRequest_ThrowsHttpRequestExceptionEx()
        {
            var handler = new FakeHandler { StatusCode = HttpStatusCode.BadRequest, Content = "bad" };
            var provider = Create(handler);

            try
            {
                await provider.GetAsync<object>("http://localhost/test");
                Assert.Fail("Expected HttpRequestExceptionEx");
            }
            catch (HttpRequestExceptionEx ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.HttpCode);
            }
        }
    }
}
