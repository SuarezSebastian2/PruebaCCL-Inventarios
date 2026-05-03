using System.ComponentModel.DataAnnotations;

namespace CclInventario.Api.Dtos;

public class CreateProductoRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MinLength(1, ErrorMessage = "Indique un nombre de al menos un carácter.")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad inicial no puede ser negativa.")]
    public int Cantidad { get; set; }
}

/// <summary>Solo nombre: el stock se modifica con movimientos (entrada/salida).</summary>
public class UpdateProductoRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MinLength(1, ErrorMessage = "Indique un nombre de al menos un carácter.")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;
}
