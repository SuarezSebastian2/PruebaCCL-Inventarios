using CclInventario.Api.Data;
using CclInventario.Api.Dtos;
using CclInventario.Api.Entities;
using CclInventario.Api.Patterns.Facade;
using CclInventario.Api.Patterns.Observer;
using CclInventario.Api.Patterns.Singleton;
using CclInventario.Api.Patterns.Strategies;
using Microsoft.EntityFrameworkCore;

namespace CclInventario.Api.Tests;

public class InventoryFacadeTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static IInventoryFacade CreateFacade(
        AppDbContext db,
        out CapturingObserver observer,
        OperationSequenceGenerator? sequence = null)
    {
        observer = new CapturingObserver();
        var notifier = new InventoryChangeNotifier(new IInventoryObserver[] { observer });
        IMovimientoStrategy[] strategies =
        [
            new EntradaMovimientoStrategy(),
            new SalidaMovimientoStrategy()
        ];
        var factory = new MovimientoStrategyFactory(strategies);
        sequence ??= new OperationSequenceGenerator();
        return new InventoryFacade(db, factory, notifier, sequence);
    }

    [Fact]
    public async Task ListInventario_returns_ordered_products()
    {
        await using var db = CreateContext();
        db.Productos.AddRange(
            new Producto { Id = 1, Nombre = "A", Cantidad = 1 },
            new Producto { Id = 2, Nombre = "B", Cantidad = 2 });
        await db.SaveChangesAsync();

        var facade = CreateFacade(db, out _);
        var list = await facade.ListInventarioAsync(CancellationToken.None);

        Assert.Equal(2, list.Count);
        Assert.Equal("A", list[0].Nombre);
        Assert.Equal("B", list[1].Nombre);
    }

    [Fact]
    public async Task RegistrarMovimiento_entrada_persists_and_notifies()
    {
        await using var db = CreateContext();
        db.Productos.Add(new Producto { Id = 1, Nombre = "P", Cantidad = 10 });
        await db.SaveChangesAsync();

        var facade = CreateFacade(db, out var observer);
        var result = await facade.RegistrarMovimientoAsync(
            new MovimientoRequest { ProductoId = 1, Tipo = "entrada", Cantidad = 3 },
            CancellationToken.None);

        Assert.True(result.Exito);
        Assert.NotNull(result.Valor);
        Assert.Equal(13, result.Valor!.Cantidad);
        Assert.Single(observer.Events);
        Assert.Equal(13, observer.Events[0].Estado.Cantidad);
        Assert.Equal("entrada", observer.Events[0].TipoMovimiento);
    }

    [Fact]
    public async Task RegistrarMovimiento_salida_conflict_returns_409()
    {
        await using var db = CreateContext();
        db.Productos.Add(new Producto { Id = 1, Nombre = "P", Cantidad = 2 });
        await db.SaveChangesAsync();

        var facade = CreateFacade(db, out var observer);
        var result = await facade.RegistrarMovimientoAsync(
            new MovimientoRequest { ProductoId = 1, Tipo = "salida", Cantidad = 5 },
            CancellationToken.None);

        Assert.False(result.Exito);
        Assert.Equal(409, result.CodigoHttp);
        Assert.Empty(observer.Events);
        Assert.Equal(2, db.Productos.Single().Cantidad);
    }

    [Fact]
    public async Task RegistrarMovimiento_unknown_tipo_returns_400()
    {
        await using var db = CreateContext();
        db.Productos.Add(new Producto { Id = 1, Nombre = "P", Cantidad = 2 });
        await db.SaveChangesAsync();

        var facade = CreateFacade(db, out var observer);
        var result = await facade.RegistrarMovimientoAsync(
            new MovimientoRequest { ProductoId = 1, Tipo = "traslado", Cantidad = 1 },
            CancellationToken.None);

        Assert.False(result.Exito);
        Assert.Equal(400, result.CodigoHttp);
        Assert.Empty(observer.Events);
    }

    [Fact]
    public async Task RegistrarMovimiento_missing_product_returns_404()
    {
        await using var db = CreateContext();

        var facade = CreateFacade(db, out var observer);
        var result = await facade.RegistrarMovimientoAsync(
            new MovimientoRequest { ProductoId = 99, Tipo = "entrada", Cantidad = 1 },
            CancellationToken.None);

        Assert.False(result.Exito);
        Assert.Equal(404, result.CodigoHttp);
        Assert.Empty(observer.Events);
    }

    private sealed class CapturingObserver : IInventoryObserver
    {
        public List<InventoryMovimientoEvent> Events { get; } = new();

        public Task OnMovimientoRegistradoAsync(InventoryMovimientoEvent evt, CancellationToken ct)
        {
            Events.Add(evt);
            return Task.CompletedTask;
        }
    }
}
