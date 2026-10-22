using System.Net;

namespace eShop.ClientApp.Services.RequestProvider
{
    public class HttpRequestExceptionEx(HttpStatusCode code, string? message, Exception? inner) : HttpRequestException(message, inner)
    {
        public HttpRequestExceptionEx(HttpStatusCode code) : this(code, null, null)
        {
        }

        public HttpRequestExceptionEx(HttpStatusCode code, string? message) : this(code, message, null)
        {
        }

        public HttpStatusCode HttpCode { get; } = code;
    }
}
