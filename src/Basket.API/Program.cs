// Composition root and startup pipeline for Basket.API.
var builder = WebApplication.CreateBuilder(args);

builder.AddBasicServiceDefaults();
builder.AddApplicationServices();

builder.Services.AddGrpc();

var app = builder.Build();

app.MapDefaultEndpoints();

// Not in upstream: lets WebApp/WebBFF (a browser SPA, no native HTTP/2
// trailer support) consume this same gRPC service via gRPC-Web, instead
// of duplicating it behind a separate REST surface.
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

app.MapGrpcService<BasketService>();

app.Run();
