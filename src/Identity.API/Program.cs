using Duende.IdentityServer.Services;
using eShop.Identity.API;
using eShop.Identity.API.Configuration;
using eShop.Identity.API.Data;
using eShop.Identity.API.Models;
using eShop.Identity.API.Services;
using eShop.ServiceDefaults;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();

builder.AddNpgsqlDbContext<ApplicationDbContext>("identitydb");

// Apply database migration automatically. Note that this approach is not
// recommended for production scenarios. Consider generating SQL scripts from
// migrations instead.
builder.Services.AddMigration<ApplicationDbContext, UsersSeed>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

builder.Services.AddIdentityServer(options =>
{
    options.Authentication.CookieLifetime = TimeSpan.FromHours(2);

    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    // TODO: Remove this line in production.
    options.KeyManagement.Enabled = false;

    // Duende's compiled-in default (confirmed via reflection: new
    // IdentityServerOptions().UserInteraction.ConsentUrl == "/consent")
    // doesn't match ConsentController's real [Route("[controller]/[action]")]
    // route. Confirmed live and load-bearing, not theoretical: with a real
    // client's RequireConsent set to true, an authenticated /connect/authorize
    // request actually redirected to "/consent" -- a route that structurally
    // cannot match "/Consent/Index" (a missing second segment, not just a
    // case difference; ASP.NET routing is case-insensitive, confirmed
    // separately for ErrorUrl's own "/home/error" default, which needed no
    // fix). LoginUrl/LogoutUrl needed no equivalent fix -- verified live via
    // real /connect/authorize and /connect/endsession requests, both already
    // correctly target /Account/Login and /Account/Logout respectively
    // (via ASP.NET Core Identity's own cookie-scheme convention, not this
    // options object at all).
    options.UserInteraction.ConsentUrl = "/Consent/Index";
})
.AddInMemoryIdentityResources(Config.GetResources())
.AddInMemoryApiScopes(Config.GetApiScopes())
.AddInMemoryApiResources(Config.GetApis())
.AddInMemoryClients(Config.GetClients(builder.Configuration))
.AddAspNetIdentity<ApplicationUser>()
// TODO: Not recommended for production - you need to store your key material somewhere secure
.AddDeveloperSigningCredential();

builder.Services.AddTransient<IProfileService, ProfileService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// This cookie policy fixes login issues with Chrome 80+ using HTTP
app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();

app.MapControllers();

app.Run();
