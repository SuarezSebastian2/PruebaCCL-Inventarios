using CclInventario.Api.Entities;

namespace CclInventario.Api.Patterns.Strategies;

public sealed class EntradaMovimientoStrategy : IMovimientoStrategy
{
    public string TipoNormalizado => "entrada";

    public MovimientoStrategyResult Apply(Producto producto, int cantidad)
    {
        producto.Cantidad += cantidad;
        return new MovimientoStrategyResult(true, false, null);
    }
}
