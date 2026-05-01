namespace CclInventario.Api.Patterns.Factory;

/// <summary>Factory Method: crea tokens JWT de sesión para un usuario autenticado.</summary>
public interface IJwtTokenFactory
{
    (string Token, int ExpiresInSeconds) CreateToken(string usuario);
}
