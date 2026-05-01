using CclInventario.Api.Entities;
using CclInventario.Api.Patterns.Strategies;

namespace CclInventario.Api.Tests;

public class MovimientoStrategyTests
{
    [Fact]
    public void Entrada_increments_quantity()
    {
        var strategy = new EntradaMovimientoStrategy();
        var producto = new Producto { Id = 1, Nombre = "P", Cantidad = 10 };

        var result = strategy.Apply(producto, 5);

        Assert.True(result.Ok);
        Assert.False(result.IsStockConflict);
        Assert.Equal(15, producto.Cantidad);
        Assert.Equal("entrada", strategy.TipoNormalizado);
    }

    [Fact]
    public void Salida_decrements_when_stock_sufficient()
    {
        var strategy = new SalidaMovimientoStrategy();
        var producto = new Producto { Id = 1, Nombre = "P", Cantidad = 10 };

        var result = strategy.Apply(producto, 4);

        Assert.True(result.Ok);
        Assert.False(result.IsStockConflict);
        Assert.Equal(6, producto.Cantidad);
        Assert.Equal("salida", strategy.TipoNormalizado);
    }

    [Fact]
    public void Salida_fails_when_insufficient_stock()
    {
        var strategy = new SalidaMovimientoStrategy();
        var producto = new Producto { Id = 1, Nombre = "P", Cantidad = 2 };

        var result = strategy.Apply(producto, 5);

        Assert.False(result.Ok);
        Assert.True(result.IsStockConflict);
        Assert.Equal(2, producto.Cantidad);
    }

    [Fact]
    public void MovimientoStrategyFactory_resolves_by_tipo_case_insensitive()
    {
        IMovimientoStrategy[] strategies =
        [
            new EntradaMovimientoStrategy(),
            new SalidaMovimientoStrategy()
        ];
        var factory = new MovimientoStrategyFactory(strategies);

        Assert.True(factory.TryGetStrategy(" ENTRADA ", out var entrada));
        Assert.NotNull(entrada);
        Assert.True(factory.TryGetStrategy("Salida", out var salida));
        Assert.NotNull(salida);
        Assert.False(factory.TryGetStrategy("traslado", out _));
    }
}
