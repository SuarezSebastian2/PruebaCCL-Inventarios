using Microsoft.IdentityModel.Tokens;

namespace CclInventario.Api.Patterns.Factory;

/// <summary>Factory Method: construye parámetros de validación JWT a partir de configuración.</summary>
public interface IJwtBearerConfigurationFactory
{
    TokenValidationParameters CreateValidationParameters();
}
