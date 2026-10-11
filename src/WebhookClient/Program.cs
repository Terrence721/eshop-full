// Placeholder entry point -- just enough for this Microsoft.NET.Sdk.Web
// project to build. Real startup wiring (cookie+OIDC auth, the
// Webhooks.API HTTP client, HooksRepository, the new SSE/login/
// webhook-JSON endpoints a React frontend needs) lands file by file
// as this project's real source is added.
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.Run();
