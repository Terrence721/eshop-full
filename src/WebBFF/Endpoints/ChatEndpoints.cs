using System.Security.Claims;
using eShop.WebBFF.Services;

namespace eShop.WebBFF.Endpoints
{
    public static class ChatEndpoints
    {
        public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
        {
            // POST /api/chat - send a message and get full response
            app.MapPost("/api/chat", async (HttpContext httpContext, IClaudeChatService chatService) =>
            {
                var tokenResult = await httpContext.GetUserAccessTokenAsync();
                if (!tokenResult.Succeeded) return Results.Unauthorized();

                var body = await httpContext.Request.ReadFromJsonAsync<ChatRequest>();
                if (string.IsNullOrEmpty(body?.Message))
                    return Results.BadRequest("Message is required");

                var buyerGuid = httpContext.User.FindFirst("sub")!.Value;
                var response = await chatService.SendUserMessageAsync(buyerGuid, body.Message);
                return Results.Ok(new { reply = response });
            })
            .RequireAuthorization()
            .AsBffApiEndpoint();

            // GET /api/chat/stream - SSE stream for real-time responses
            app.MapGet("/api/chat/stream", async (HttpContext httpContext, IClaudeChatService chatService, CancellationToken ct) =>
            {
                var tokenResult = await httpContext.GetUserAccessTokenAsync();
                if (!tokenResult.Succeeded) return Results.Unauthorized();

                var query = httpContext.Request.Query["message"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(query))
                    return Results.BadRequest("Message query parameter is required");

                var buyerGuid = httpContext.User.FindFirst("sub")!.Value;
                httpContext.Response.Headers.ContentType = "text/event-stream";
                httpContext.Response.Headers.CacheControl = "no-cache";

                await foreach (var delta in chatService.StreamAssistantResponseAsync(buyerGuid, query, ct))
                {
                    await httpContext.Response.WriteAsync($"data: {delta}\n\n", ct);
                    await httpContext.Response.Body.FlushAsync(ct);
                }
                await httpContext.Response.Body.FlushAsync(ct);
                return Results.Empty;
            })
            .RequireAuthorization();

            // POST /api/chat/clear - clear conversation history
            app.MapPost("/api/chat/clear", async (HttpContext httpContext, IClaudeChatService chatService) =>
            {
                var tokenResult = await httpContext.GetUserAccessTokenAsync();
                if (!tokenResult.Succeeded) return Results.Unauthorized();

                var buyerGuid = httpContext.User.FindFirst("sub")!.Value;
                chatService.ClearConversation(buyerGuid);
                return Results.Ok();
            })
            .RequireAuthorization()
            .AsBffApiEndpoint();

            return app;
        }
    }

    internal record ChatRequest(string? Message);
}
