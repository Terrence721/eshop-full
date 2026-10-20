var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddProblemDetails();

var withApiVersioning = builder.Services.AddApiVersioning(options =>
{
    // Include "api-supported-versions" and "api-deprecated-versions" headers in all responses
    options.ReportApiVersions = true;
});

builder.AddDefaultOpenApi(withApiVersioning);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseStatusCodePages();

var webHooks = app.NewVersionedApi("Web Hooks");

webHooks.MapWebHooksApiV1()
        .RequireAuthorization();

app.UseDefaultOpenApi();
app.Run();
