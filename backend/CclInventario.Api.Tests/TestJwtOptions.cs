using CclInventario.Api.Options;
using Microsoft.Extensions.Options;

namespace CclInventario.Api.Tests;

internal static class TestJwtOptions
{
    internal static IOptions<JwtOptions> CreateValidOptions() =>
        Microsoft.Extensions.Options.Options.Create(new JwtOptions
        {
            Key = "CCL-Prueba-Tests-Clave-Simetrica-Minimo-32-Bytes!",
            Issuer = "Tests",
            Audience = "Tests.Client",
            ExpiresMinutes = 60
        });
}
