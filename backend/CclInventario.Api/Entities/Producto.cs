namespace CclInventario.Api.Entities;

/// <summary>
/// Producto almacenado en inventario (tabla única según requisitos).
/// </summary>
public class Producto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int Cantidad { get; set; }
}
