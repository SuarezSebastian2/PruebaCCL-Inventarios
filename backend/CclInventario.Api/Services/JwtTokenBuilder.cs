using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CclInventario.Api.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CclInventario.Api.Services;

public class JwtTokenBuilder
{
    private readonly JwtOptions _options;

    public JwtTokenBuilder(IOptions<JwtOptions> options) =>
        _options = options.Value;

    public (string Token, int ExpiresInSeconds) CreateToken(string usuario)
    {
        var expires = DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes);
        var expiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario),
            new Claim(JwtRegisteredClaimNames.UniqueName, usuario),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, Math.Max(expiresIn, 60));
    }
}
