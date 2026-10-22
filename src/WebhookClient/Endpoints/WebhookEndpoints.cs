using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace eShop.WebhookClient.Endpoints;

/// <summary>
/// Webhook receiving endpoints invoked by Webhooks.API.
/// </summary>
public static class WebhookEndpoints
{
    /// <summary>
    /// Header carrying the shared webhook token.
    /// </summary>
    public const string WebhookCheckHeader = "X-eshop-whtoken";

    /// <summary>
    /// Maps the OPTIONS check and POST receive endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        var configuration = app.ServiceProvider.GetRequiredService<IConfiguration>();
        bool.TryParse(configuration["ValidateToken"], out var validateToken);
        var tokenToValidate = configuration["WebhookClientOptions:Token"];

        app.MapMethods("/check", [HttpMethods.Options], ([FromHeader(Name = WebhookCheckHeader)] string value, HttpResponse response) =>
            CheckToken(value, response, validateToken, tokenToValidate));

        app.MapPost("/webhook-received", (WebhookData hook, HttpRequest request, ILogger<Program> logger, HooksRepository hooksRepository) =>
            ReceiveWebhookAsync(hook, request, logger, hooksRepository, validateToken, tokenToValidate));

        return app;
    }

    /// <summary>
    /// Validates/gates the shared token for a grant OPTIONS request.
    /// </summary>
    public static Results<Ok, BadRequest<string>> CheckToken(string value, HttpResponse response, bool validateToken, string? tokenToValidate)
    {
        if (!validateToken || value == tokenToValidate)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                response.Headers.Append(WebhookCheckHeader, value);
            }

            return TypedResults.Ok();
        }

        return TypedResults.BadRequest("Invalid token");
    }

    /// <summary>
    /// Receives a posted webhook payload, validates the token, and stores it.
    /// </summary>
    public static async Task<IResult> ReceiveWebhookAsync(WebhookData hook, HttpRequest request, ILogger<Program> logger, HooksRepository hooksRepository, bool validateToken, string? tokenToValidate)
    {
        var token = request.Headers[WebhookCheckHeader];

        logger.LogInformation("Received hook with token {Token}. My token is {MyToken}. Token validation is set to {ValidateToken}", LogSanitizer.ForLog(token.ToString()), tokenToValidate, validateToken);

        if (!validateToken || tokenToValidate == token)
        {
            logger.LogInformation("Received hook is going to be processed");
            var newHook = new WebHookReceived()
            {
                Data = hook.Payload,
                When = hook.When,
                Token = token
            };
            await hooksRepository.AddNew(newHook);
            logger.LogInformation("Received hook was processed.");
            return Results.Ok(newHook);
        }

        logger.LogInformation("Received hook is NOT processed - Bad Request returned.");
        return Results.BadRequest();
    }
}