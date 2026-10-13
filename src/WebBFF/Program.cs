// Placeholder entry point -- just enough for this Microsoft.NET.Sdk.Web
// project to build. Real startup wiring (AddServiceDefaults, Duende.BFF's
// OIDC/cookie setup, YARP proxying to Catalog.API/Basket.API/Ordering.API,
// the order-status-push subscriptions) lands file by file as this
// project's real source is added, matching every prior project's
// precedent.
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.Run();
