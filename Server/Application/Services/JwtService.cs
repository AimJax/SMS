using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Account account)
    {
        var key = GetSecurityKey();
        var issuer = _configuration["Jwt:Issuer"] ?? "SocialMediaSimulator";
        var audience = _configuration["Jwt:Audience"] ?? "SocialMediaSimulator";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("account_id", account.AccountId.ToString()),
            new Claim(ClaimTypes.Name, account.Username),
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString())
        };

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int? GetAccountIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = GetSecurityKey();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "SocialMediaSimulator",
                ValidAudience = _configuration["Jwt:Audience"] ?? "SocialMediaSimulator",
                IssuerSigningKey = key
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

            return idClaim != null ? int.Parse(idClaim.Value) : null;
        }
        catch
        {
            return null;
        }
    }

    private SymmetricSecurityKey GetSecurityKey()
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured");

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }
}
