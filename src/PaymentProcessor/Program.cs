// Placeholder entry point -- just enough for this Microsoft.NET.Sdk.Web
// project to build. Real startup wiring (AddServiceDefaults, the RabbitMQ
// event bus subscription, PaymentOptions) lands file by file as this
// project's real source is added, matching OrderProcessor's precedent.
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.Run();
