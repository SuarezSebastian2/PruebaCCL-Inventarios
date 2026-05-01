using CclInventario.Api.Data;
using CclInventario.Api.Dtos;
using CclInventario.Api.Patterns.Observer;
using CclInventario.Api.Patterns.Singleton;
using CclInventario.Api.Patterns.Strategies;
using Microsoft.EntityFrameworkCore;

namespace CclInventario.Api.Patterns.Facade;

/// <summary>
/// GoF Facade: orquesta persistencia, <see cref="IMovimientoStrategyFactory"/>, notificación a observadores y correlación.
/// </summary>
public sealed class InventoryFacade : IInventoryFacade
{
    private readonly AppDbContext _db;
    private readonly IMovimientoStrategyFactory _strategyFactory;
    private readonly IInventoryChangeNotifier _notifier;
    private readonly OperationSequenceGenerator _sequence;

    public InventoryFacade(
        AppDbContext db,
        IMovimientoStrategyFactory strategyFactory,
        IInventoryChangeNotifier notifier,
        OperationSequenceGenerator sequence)
    {
        _db = db;
        _strategyFactory = strategyFactory;
        _notifier = notifier;
        _sequence = sequence;
    }

    public async Task<IReadOnlyList<ProductoInventarioDto>> ListInventarioAsync(CancellationToken ct)
    {
        var list = await _db.Productos
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new ProductoInventarioDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Cantidad = p.Cantidad
            })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return list;
    }

    public async Task<InventoryFacadeMovimientoResult> RegistrarMovimientoAsync(
        MovimientoRequest request,
        CancellationToken ct)
    {
        if (!_strategyFactory.TryGetStrategy(request.Tipo, out var strategy) || strategy is null)
        {
            return InventoryFacadeMovimientoResult.BadRequest(
                new { message = "Tipo debe ser 'entrada' o 'salida'." });
        }

        var producto = await _db.Productos
            .FirstOrDefaultAsync(p => p.Id == request.ProductoId, ct)
            .ConfigureAwait(false);

        if (producto is null)
        {
            return InventoryFacadeMovimientoResult.NotFound(
                new { message = $"No existe producto con id {request.ProductoId}." });
        }

        var outcome = strategy.Apply(producto, request.Cantidad);
        if (!outcome.Ok)
        {
            if (outcome.IsStockConflict)
            {
                return InventoryFacadeMovimientoResult.Conflict(new
                {
                    message = outcome.Message,
                    disponible = producto.Cantidad
                });
            }

            return InventoryFacadeMovimientoResult.BadRequest(new { message = outcome.Message });
        }

        await _db.SaveChangesAsync(ct).ConfigureAwait(false);

        var dto = new ProductoInventarioDto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Cantidad = producto.Cantidad
        };

        var evt = new InventoryMovimientoEvent
        {
            Estado = dto,
            TipoMovimiento = strategy.TipoNormalizado,
            Cantidad = request.Cantidad,
            OperacionSecuencial = _sequence.Next()
        };

        await _notifier.NotifyMovimientoRegistradoAsync(evt, ct).ConfigureAwait(false);

        return InventoryFacadeMovimientoResult.Success(dto);
    }
}
