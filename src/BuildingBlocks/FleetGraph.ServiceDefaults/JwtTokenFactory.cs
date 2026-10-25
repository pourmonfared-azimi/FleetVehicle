using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FleetGraph.ServiceDefaults;

/// <summary>Issues demo tokens (used by the gateway dev endpoint and by the tests).</summary>
public static class JwtTokenFactory
{
    public static string CreateToken(
        JwtOptions options,
        string subject,
        IEnumerable<string> roles,
        TimeSpan? lifetime = null)
    {
        var claims = new List<Claim> { new(FleetGraphAuth.NameClaimType, subject) };
        claims.AddRange(roles.Select(r => new Claim(FleetGraphAuth.RoleClaimType, r)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = options.Issuer,
            Audience = options.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(8)),
            SigningCredentials = new SigningCredentials(
                CreateSigningKey(options),
                SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public static SymmetricSecurityKey CreateSigningKey(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.SigningKey) || options.SigningKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must be configured and be at least 32 characters long.");
        }

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
    }
}
