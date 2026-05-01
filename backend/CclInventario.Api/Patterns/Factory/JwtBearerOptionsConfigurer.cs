using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace CclInventario.Api.Patterns.Factory;

/// <summary>
/// Conecta la <see cref="IJwtBearerConfigurationFactory"/> con el pipeline de autenticación JWT (opciones nombradas).
/// </summary>
public sealed class JwtBearerOptionsConfigurer : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly IJwtBearerConfigurationFactory _factory;

    public JwtBearerOptionsConfigurer(IJwtBearerConfigurationFactory factory) =>
        _factory = factory;

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name == JwtBearerDefaults.AuthenticationScheme)
        {
            options.TokenValidationParameters = _factory.CreateValidationParameters();
        }
    }

    public void Configure(JwtBearerOptions options) =>
        Configure(JwtBearerDefaults.AuthenticationScheme, options);
}
