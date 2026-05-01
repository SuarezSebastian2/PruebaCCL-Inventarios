using CclInventario.Api.Entities;

namespace CclInventario.Api.Patterns.Strategies;

/// <summary>GoF Strategy: algoritmo de aplicación de movimiento sobre un <see cref="Producto"/>.</summary>
public interface IMovimientoStrategy
{
    string TipoNormalizado { get; }

    MovimientoStrategyResult Apply(Producto producto, int cantidad);
}

public sealed record MovimientoStrategyResult(
    bool Ok,
    bool IsStockConflict,
    string? Message);
