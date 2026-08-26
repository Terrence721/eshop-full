using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using eShop.Identity.API.Models;

namespace eShop.Identity.API.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context, CancellationToken cancellationToken)
    {
        var subjectId = context.Subject.Claims.Where(x => x.Type == "sub").FirstOrDefault()?.Value
            ?? throw new ArgumentException("Invalid subject identifier");

        var user = await _userManager.FindByIdAsync(subjectId)
            ?? throw new ArgumentException("Invalid subject identifier");

        var claims = GetClaimsFromUser(user);
        context.IssuedClaims = claims.ToList();
    }

    public async Task IsActiveAsync(IsActiveContext context, CancellationToken cancellationToken)
    {
        context.IsActive = false;

        var subjectId = context.Subject.Claims.Where(x => x.Type == "sub").FirstOrDefault()?.Value;
        if (subjectId == null)
        {
            return;
        }

        var user = await _userManager.FindByIdAsync(subjectId);
        if (user == null)
        {
            return;
        }

        if (_userManager.SupportsUserSecurityStamp)
        {
            var securityStamp = context.Subject.Claims.Where(c => c.Type == "security_stamp").Select(c => c.Value).SingleOrDefault();
            if (securityStamp != null)
            {
                var dbSecurityStamp = await _userManager.GetSecurityStampAsync(user);
                if (dbSecurityStamp != securityStamp)
                {
                    return;
                }
            }
        }

        context.IsActive =
            !user.LockoutEnabled ||
            !user.LockoutEnd.HasValue ||
            user.LockoutEnd <= DateTime.UtcNow;
    }

    private IEnumerable<Claim> GetClaimsFromUser(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Subject, user.Id),
            new(JwtClaimTypes.PreferredUserName, user.UserName ?? user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? user.Id)
        };

        if (!string.IsNullOrWhiteSpace(user.Name))
            claims.Add(new Claim("name", user.Name));

        if (!string.IsNullOrWhiteSpace(user.LastName))
            claims.Add(new Claim("last_name", user.LastName));

        if (!string.IsNullOrWhiteSpace(user.CardNumber))
            claims.Add(new Claim("card_number", user.CardNumber));

        if (!string.IsNullOrWhiteSpace(user.CardHolderName))
            claims.Add(new Claim("card_holder", user.CardHolderName));

        if (!string.IsNullOrWhiteSpace(user.SecurityNumber))
            claims.Add(new Claim("card_security_number", user.SecurityNumber));

        if (!string.IsNullOrWhiteSpace(user.Expiration))
            claims.Add(new Claim("card_expiration", user.Expiration));

        if (!string.IsNullOrWhiteSpace(user.City))
            claims.Add(new Claim("address_city", user.City));

        if (!string.IsNullOrWhiteSpace(user.Country))
            claims.Add(new Claim("address_country", user.Country));

        if (!string.IsNullOrWhiteSpace(user.State))
            claims.Add(new Claim("address_state", user.State));

        if (!string.IsNullOrWhiteSpace(user.Street))
            claims.Add(new Claim("address_street", user.Street));

        if (!string.IsNullOrWhiteSpace(user.ZipCode))
            claims.Add(new Claim("address_zip_code", user.ZipCode));

        // Guarded with IsNullOrWhiteSpace, matching the PhoneNumber block below -
        // upstream guarded PhoneNumber but not Email despite both being nullable,
        // an inconsistency rather than a deliberate difference.
        if (_userManager.SupportsUserEmail && !string.IsNullOrWhiteSpace(user.Email))
        {
            claims.AddRange(
            [
                new Claim(JwtClaimTypes.Email, user.Email),
                new Claim(JwtClaimTypes.EmailVerified, user.EmailConfirmed ? "true" : "false", ClaimValueTypes.Boolean)
            ]);
        }

        if (_userManager.SupportsUserPhoneNumber && !string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            claims.AddRange(
            [
                new Claim(JwtClaimTypes.PhoneNumber, user.PhoneNumber),
                new Claim(JwtClaimTypes.PhoneNumberVerified, user.PhoneNumberConfirmed ? "true" : "false", ClaimValueTypes.Boolean)
            ]);
        }

        return claims;
    }
}
