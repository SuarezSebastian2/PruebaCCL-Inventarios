using CclInventario.Api.Dtos;

namespace CclInventario.Api.Patterns.Observer;

/// <summary>Evento publicado tras un movimiento de inventario exitoso (GoF Observer).</summary>
public sealed class InventoryMovimientoEvent
{
    public required ProductoInventarioDto Estado { get; init; }

    public required string TipoMovimiento { get; init; }

    public int Cantidad { get; init; }

    public long OperacionSecuencial { get; init; }
}
