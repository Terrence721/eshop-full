using eShop.Identity.API.Data;
using eShop.Identity.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace eShop.Identity.API;

public class UsersSeed(ILogger<UsersSeed> logger, UserManager<ApplicationUser> userManager) : IDbSeeder<ApplicationDbContext>
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        await EnsureUserAsync("alice", "Pass123$", new ApplicationUser
        {
            UserName = "alice",
            Email = "AliceSmith@email.com",
            EmailConfirmed = true,
            CardHolderName = "Alice Smith",
            CardNumber = "XXXXXXXXXXXX1881",
            CardType = 1,
            City = "Redmond",
            Country = "U.S.",
            Expiration = "12/24",
            Id = Guid.NewGuid().ToString(),
            LastName = "Smith",
            Name = "Alice",
            PhoneNumber = "1234567890",
            ZipCode = "98052",
            State = "WA",
            Street = "15703 NE 61st Ct",
            SecurityNumber = "123"
        });

        await EnsureUserAsync("bob", "Pass123$", new ApplicationUser
        {
            UserName = "bob",
            Email = "BobSmith@email.com",
            EmailConfirmed = true,
            CardHolderName = "Bob Smith",
            CardNumber = "XXXXXXXXXXXX1881",
            CardType = 1,
            City = "Redmond",
            Country = "U.S.",
            Expiration = "12/24",
            Id = Guid.NewGuid().ToString(),
            LastName = "Smith",
            Name = "Bob",
            PhoneNumber = "1234567890",
            ZipCode = "98052",
            State = "WA",
            Street = "15703 NE 61st Ct",
            SecurityNumber = "456"
        });
    }

    private async Task EnsureUserAsync(string userName, string password, ApplicationUser user)
    {
        var existing = await userManager.FindByNameAsync(userName);
        if (existing != null)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("{UserName} already exists", userName);
            }
            return;
        }

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new Exception(result.Errors.First().Description);
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("{UserName} created", userName);
        }
    }
}
