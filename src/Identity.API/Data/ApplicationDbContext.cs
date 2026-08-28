using eShop.Identity.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace eShop.Identity.API.Data;

/// <remarks>
/// Add migrations using the following command inside the 'Identity.API' project directory:
///
/// dotnet ef migrations add [migration-name]
/// </remarks>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ApplicationUser's extended profile properties are string? -- deliberately nullable
        // since ExternalController.AutoProvisionUserAsync's real construction path leaves them
        // all unset. [Required] stays on each property for its own purpose (ASP.NET Core model
        // validation on any future endpoint that lets a user submit this data), but EF Core's
        // convention would otherwise also read [Required] as .IsRequired() for the database
        // schema -- overridden here so the column nullability actually matches the C# type.
        var user = builder.Entity<ApplicationUser>();
        user.Property(u => u.CardNumber).IsRequired(false);
        user.Property(u => u.SecurityNumber).IsRequired(false);
        user.Property(u => u.Expiration).IsRequired(false);
        user.Property(u => u.CardHolderName).IsRequired(false);
        user.Property(u => u.Street).IsRequired(false);
        user.Property(u => u.City).IsRequired(false);
        user.Property(u => u.State).IsRequired(false);
        user.Property(u => u.Country).IsRequired(false);
        user.Property(u => u.ZipCode).IsRequired(false);
        user.Property(u => u.Name).IsRequired(false);
        user.Property(u => u.LastName).IsRequired(false);
    }
}
