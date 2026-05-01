using System.Text;
using CclInventario.Api.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CclInventario.Api.Patterns.Factory;

/// <summary>
/// GoF Factory: centraliza la creación de <see cref="TokenValidationParameters"/> (singleton en DI).
/// </summary>
public sealed class JwtBearerConfigurationFactory : IJwtBearerConfigurationFactory
{
    private readonly JwtOptions _options;

    public JwtBearerConfigurationFactory(IOptions<JwtOptions> options) =>
        _options = options.Value;

    public TokenValidationParameters CreateValidationParameters() =>
        new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key))
        };
}
