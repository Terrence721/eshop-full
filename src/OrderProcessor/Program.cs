// Placeholder entry point -- just enough for this Microsoft.NET.Sdk.Worker
// project to build. Real startup wiring (AddBasicServiceDefaults, the
// RabbitMQ event bus, the Npgsql data source, GracePeriodManagerService)
// lands file by file as this project's real source is added, matching
// Basket.API/Catalog.API's precedent.
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.Run();
