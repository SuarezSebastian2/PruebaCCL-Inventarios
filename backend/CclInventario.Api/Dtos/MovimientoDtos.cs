using System.ComponentModel.DataAnnotations;

namespace CclInventario.Api.Dtos;

public class MovimientoRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un producto válido.")]
    public int ProductoId { get; set; }

    /// <summary>entrada o salida (insensible a mayúsculas).</summary>
    [Required(ErrorMessage = "El tipo de movimiento es obligatorio.")]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }
}

public class ProductoInventarioDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int Cantidad { get; set; }
}
