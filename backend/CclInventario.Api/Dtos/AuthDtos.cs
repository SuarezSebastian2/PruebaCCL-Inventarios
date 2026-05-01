using System.ComponentModel.DataAnnotations;

namespace CclInventario.Api.Dtos;

public class LoginRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La clave es obligatoria.")]
    public string Clave { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public int ExpiresInSeconds { get; set; }

    public string TokenType { get; set; } = "Bearer";
}
