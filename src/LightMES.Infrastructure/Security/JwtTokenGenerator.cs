using LightMES.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LightMES.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _config;
    private readonly IPermissionService _permissionService;

    public JwtTokenGenerator(IConfiguration config, IPermissionService permissionService)
    {
        _config = config;
        _permissionService = permissionService;
    }

    public string GenerateToken(Guid userId, string username, string fullName, IEnumerable<string> roles)
    {
        var secretKey = _config["JwtSettings:Secret"] ?? "SuperSecretKeyForLightMES_DontSHareIt_123456789";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim("full_name", fullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"] ?? "LightMES",
            audience: _config["JwtSettings:Audience"] ?? "MES_Clients",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds
            );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
