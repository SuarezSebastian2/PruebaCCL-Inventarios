using System.ComponentModel.DataAnnotations;

namespace CclInventario.Api.Dtos;

public class LoginRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [MaxLength(128, ErrorMessage = "El usuario no puede superar 128 caracteres.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La clave es obligatoria.")]
    [MaxLength(256, ErrorMessage = "La clave no puede superar 256 caracteres.")]
    public string Clave { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public int ExpiresInSeconds { get; set; }

    public string TokenType { get; set; } = "Bearer";
}
