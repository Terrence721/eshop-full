// Basket.API's native gRPC endpoint is plain HTTP/2 (no TLS) -- the .NET gRPC client
// refuses unencrypted HTTP/2 by default, so this must be set before any channel is
// created.
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var identityUrl = builder.Configuration.GetRequiredValue("IdentityUrl");
var basketUrl = builder.Configuration.GetRequiredValue("BasketUrl");

builder.Services.AddBff()
    .ConfigureOpenIdConnect(options =>
    {
        options.Authority = identityUrl;
        options.ClientId = "webapp";
        options.ClientSecret = "secret"; // matches Identity.API's Config.cs webapp client
        options.ResponseType = "code";
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("offline_access");
        options.Scope.Add("orders");
        options.Scope.Add("basket");
    })
    .ConfigureCookies(options =>
    {
        // Must be distinct from every other service's cookie name, otherwise they'll
        // interfere with each other when all running on localhost (same pattern already
        // established for WebhookClient/Identity.WebApp).
        options.Cookie.Name = ".AspNetCore.WebBFFIdentity";
        options.Cookie.SameSite = SameSiteMode.Lax;
    })
    .AddRemoteApis();

builder.Services.AddReverseProxy()
    .AddBffExtensions()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSingleton<OrderStatusNotifications>();

builder.Services.AddGrpcClient<BasketGrpcClient>(options => options.Address = new Uri(basketUrl));
builder.Services.AddScoped<BasketService>();

builder.AddRabbitMqEventBus("eventbus")
    .AddSubscription<OrderStatusChangedToSubmittedIntegrationEvent, OrderStatusChangedToSubmittedIntegrationEventHandler>()
    .AddSubscription<OrderStatusChangedToAwaitingValidationIntegrationEvent, OrderStatusChangedToAwaitingValidationIntegrationEventHandler>()
    .AddSubscription<OrderStatusChangedToPaidIntegrationEvent, OrderStatusChangedToPaidIntegrationEventHandler>()
    .AddSubscription<OrderStatusChangedToStockConfirmedIntegrationEvent, OrderStatusChangedToStockConfirmedIntegrationEventHandler>()
    .AddSubscription<OrderStatusChangedToShippedIntegrationEvent, OrderStatusChangedToShippedIntegrationEventHandler>()
    .AddSubscription<OrderStatusChangedToCancelledIntegrationEvent, OrderStatusChangedToCancelledIntegrationEventHandler>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseRouting();
app.UseBff();
app.UseAuthorization();

app.MapReverseProxy().AsBffApiEndpoint();
app.MapOrderStatusEndpoints();
app.MapBasketEndpoints();

await app.RunAsync();
