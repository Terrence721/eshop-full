using eShop.Identity.API.Data;
using eShop.Identity.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace eShop.Identity.API;

public class UsersSeed(ILogger<UsersSeed> logger, UserManager<ApplicationUser> userManager) : IDbSeeder<ApplicationDbContext>
{
    private const string SeedPassword = "Pass123$";

    public async Task SeedAsync(ApplicationDbContext context)
    {
        foreach (var (userName, user) in GetSeedUsers())
        {
            await EnsureUserAsync(userName, user);
        }
    }

    private static IEnumerable<(string UserName, ApplicationUser User)> GetSeedUsers()
    {
        yield return ("alice", CreateUser("alice", "Alice", "Smith", "AliceSmith@email.com", "Redmond", "WA", "15703 NE 61st Ct", "98052", "1881", "123"));
        yield return ("bob", CreateUser("bob", "Bob", "Smith", "BobSmith@email.com", "Redmond", "WA", "15703 NE 61st Ct", "98052", "1881", "456"));
        yield return ("charlie", CreateUser("charlie", "Charlie", "Davis", "CharlieDavis@email.com", "Seattle", "WA", "400 Broad St", "98109", "2004", "789"));
        yield return ("diana", CreateUser("diana", "Diana", "Evans", "DianaEvans@email.com", "Portland", "OR", "1120 SW 5th Ave", "97204", "3157", "234"));
        yield return ("ethan", CreateUser("ethan", "Ethan", "Foster", "EthanFoster@email.com", "San Francisco", "CA", "1 Market St", "94105", "4488", "567"));
        yield return ("fiona", CreateUser("fiona", "Fiona", "Garcia", "FionaGarcia@email.com", "Austin", "TX", "500 Congress Ave", "78701", "5729", "890"));
        yield return ("george", CreateUser("george", "George", "Harris", "GeorgeHarris@email.com", "Chicago", "IL", "233 S Wacker Dr", "60606", "6031", "345"));
        yield return ("hannah", CreateUser("hannah", "Hannah", "Irving", "HannahIrving@email.com", "Denver", "CO", "1144 15th St", "80202", "7362", "678"));
        yield return ("ian", CreateUser("ian", "Ian", "Johnson", "IanJohnson@email.com", "Boston", "MA", "1 Federal St", "02110", "8493", "901"));
        yield return ("julia", CreateUser("julia", "Julia", "King", "JuliaKing@email.com", "New York", "NY", "350 5th Ave", "10118", "9624", "112"));
    }

    private static ApplicationUser CreateUser(
        string userName, string firstName, string lastName, string email,
        string city, string state, string street, string zipCode,
        string cardNumberLastFour, string securityNumber) => new()
    {
        Id = Guid.NewGuid().ToString(),
        UserName = userName,
        Email = email,
        EmailConfirmed = true,
        Name = firstName,
        LastName = lastName,
        CardHolderName = $"{firstName} {lastName}",
        CardNumber = $"XXXXXXXXXXXX{cardNumberLastFour}",
        CardType = 1,
        Expiration = "12/24",
        SecurityNumber = securityNumber,
        PhoneNumber = "1234567890",
        City = city,
        State = state,
        Street = street,
        ZipCode = zipCode,
        Country = "U.S."
    };

    private async Task EnsureUserAsync(string userName, ApplicationUser user)
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

        var result = await userManager.CreateAsync(user, SeedPassword);
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
