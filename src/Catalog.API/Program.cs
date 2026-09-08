// Placeholder entry point -- just enough for this Microsoft.NET.Sdk.Web
// project to build. Real startup wiring (AddApplicationServices, API
// versioning, OpenAPI, endpoint mapping) lands file by file as this
// project's real source is added, matching Identity.API's precedent.
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.Run();
