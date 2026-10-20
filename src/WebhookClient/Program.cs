var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapAuthenticationEndpoints();
app.MapWebhookEndpoints();
app.MapBffEndpoints();
app.MapHooksApiEndpoints();

app.Run();
