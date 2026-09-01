using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymProgress.Application;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GymProgress.Infrastructure;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    public string Create(Guid userId, string email, string displayName)
    {
        var settings = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(RequireKey(settings.Key)));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(Math.Clamp(settings.ExpirationDays, 1, 90));

        var token = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Name, displayName)
            ],
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string RequireKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException("Jwt:Key måste vara minst 32 tecken.");
        }

        return key;
    }
}
