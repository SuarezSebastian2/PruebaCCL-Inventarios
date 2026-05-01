using CclInventario.Api.Entities;

namespace CclInventario.Api.Patterns.Strategies;

public sealed class SalidaMovimientoStrategy : IMovimientoStrategy
{
    public string TipoNormalizado => "salida";

    public MovimientoStrategyResult Apply(Producto producto, int cantidad)
    {
        if (producto.Cantidad < cantidad)
        {
            return new MovimientoStrategyResult(
                false,
                true,
                "Stock insuficiente para la salida solicitada.");
        }

        producto.Cantidad -= cantidad;
        return new MovimientoStrategyResult(true, false, null);
    }
}
