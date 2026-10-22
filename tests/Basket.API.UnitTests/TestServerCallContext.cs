using Grpc.Core;
using Microsoft.AspNetCore.Http;

namespace eShop.Basket.API.UnitTests
{
    // Grpc.Core.Testing.TestServerCallContext has no concept of an ASP.NET Core
    // HttpContext (it predates Grpc.AspNetCore), so BasketService's real calls to
    // ServerCallContextExtensions.GetHttpContext() need a hand-built double instead.
    // ServerCallContext's "Core" members are abstract except UserStateCore, which
    // already has a working virtual default (a plain in-memory dictionary) -
    // confirmed via reflection, not assumed. GetHttpContext() reads that dictionary
    // for the "__HttpContext" key - also confirmed via reflection, since it isn't
    // publicly documented.
    internal sealed class TestServerCallContext : ServerCallContext
    {
        public static ServerCallContext Create(HttpContext? httpContext = null)
        {
            var context = new TestServerCallContext();
            context.UserState["__HttpContext"] = httpContext ?? new DefaultHttpContext();
            return context;
        }

        protected override string MethodCore => "test-method";
        protected override string HostCore => "test-host";
        protected override string PeerCore => "test-peer";
        protected override DateTime DeadlineCore => DateTime.MaxValue;
        protected override Metadata RequestHeadersCore => [];
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override Metadata ResponseTrailersCore => [];
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore => new("test-peer", []);

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) =>
            throw new NotSupportedException("Not needed by any test in this project.");
    }
}
