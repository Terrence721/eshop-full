namespace eShop.WebBFF.Endpoints;

public static class BasketEndpoints
{
    public static IEndpointRouteBuilder MapBasketEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/basket", async (HttpContext httpContext, BasketService basket) =>
        {
            var tokenResult = await httpContext.GetUserAccessTokenAsync();
            if (!tokenResult.Succeeded)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(await basket.GetBasketAsync(tokenResult.Token.AccessToken));
        })
        .RequireAuthorization()
        .AsBffApiEndpoint();

        // Always a full replace, matching UpdateBasketRequest's own gRPC semantics --
        // the caller resends the whole basket, not a single line delta.
        app.MapPut("/api/basket", async (HttpContext httpContext, BasketService basket, IReadOnlyCollection<BasketQuantity> items) =>
        {
            var tokenResult = await httpContext.GetUserAccessTokenAsync();
            if (!tokenResult.Succeeded)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(await basket.UpdateBasketAsync(tokenResult.Token.AccessToken, items));
        })
        .RequireAuthorization()
        .AsBffApiEndpoint();

        app.MapDelete("/api/basket", async (HttpContext httpContext, BasketService basket) =>
        {
            var tokenResult = await httpContext.GetUserAccessTokenAsync();
            if (!tokenResult.Succeeded)
            {
                return Results.Unauthorized();
            }

            await basket.DeleteBasketAsync(tokenResult.Token.AccessToken);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .AsBffApiEndpoint();

        return app;
    }
}
