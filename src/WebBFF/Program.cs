var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var identityUrl = builder.Configuration.GetRequiredValue("IdentityUrl");

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

await app.RunAsync();
