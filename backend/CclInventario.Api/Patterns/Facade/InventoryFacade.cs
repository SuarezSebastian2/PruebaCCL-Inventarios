using CclInventario.Api.Data;
using CclInventario.Api.Dtos;
using CclInventario.Api.Entities;
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

    public async Task<ProductoInventarioDto?> GetProductoByIdAsync(int id, CancellationToken ct)
    {
        var p = await _db.Productos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return null;
        }

        return new ProductoInventarioDto { Id = p.Id, Nombre = p.Nombre, Cantidad = p.Cantidad };
    }

    public async Task<InventoryFacadeCrudResult> CrearProductoAsync(CreateProductoRequest request, CancellationToken ct)
    {
        var nombreNorm = NormalizeNombre(request.Nombre);
        if (nombreNorm.Length == 0)
        {
            return InventoryFacadeCrudResult.BadRequest(new { message = "El nombre no puede quedar vacío." });
        }

        if (await ExistsNombreAsync(nombreNorm, excludeId: null, ct).ConfigureAwait(false))
        {
            return InventoryFacadeCrudResult.Conflict(new { message = "Ya existe un producto con ese nombre." });
        }

        var entity = new Producto { Nombre = nombreNorm, Cantidad = request.Cantidad };
        _db.Productos.Add(entity);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);

        return InventoryFacadeCrudResult.Created(ToDto(entity));
    }

    public async Task<InventoryFacadeCrudResult> ActualizarProductoAsync(int id, UpdateProductoRequest request, CancellationToken ct)
    {
        var nombreNorm = NormalizeNombre(request.Nombre);
        if (nombreNorm.Length == 0)
        {
            return InventoryFacadeCrudResult.BadRequest(new { message = "El nombre no puede quedar vacío." });
        }

        var entity = await _db.Productos.FirstOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        if (entity is null)
        {
            return InventoryFacadeCrudResult.NotFound(new { message = $"No existe producto con id {id}." });
        }

        if (await ExistsNombreAsync(nombreNorm, excludeId: id, ct).ConfigureAwait(false))
        {
            return InventoryFacadeCrudResult.Conflict(new { message = "Ya existe otro producto con ese nombre." });
        }

        entity.Nombre = nombreNorm;
        entity.Cantidad = request.Cantidad;
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);

        return InventoryFacadeCrudResult.Ok(ToDto(entity));
    }

    public async Task<InventoryFacadeCrudResult> EliminarProductoAsync(int id, CancellationToken ct)
    {
        var entity = await _db.Productos.FirstOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        if (entity is null)
        {
            return InventoryFacadeCrudResult.NotFound(new { message = $"No existe producto con id {id}." });
        }

        _db.Productos.Remove(entity);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);

        return InventoryFacadeCrudResult.Deleted();
    }

    private static string NormalizeNombre(string nombre) => nombre.Trim();

    private async Task<bool> ExistsNombreAsync(string nombreNormalized, int? excludeId, CancellationToken ct)
    {
        var key = nombreNormalized.ToUpperInvariant();
        return await _db.Productos
            .AsNoTracking()
            .Where(p => excludeId == null || p.Id != excludeId)
            .AnyAsync(p => p.Nombre.ToUpper() == key, ct)
            .ConfigureAwait(false);
    }

    /// <summary>Nombres se comparan sin distinguir mayúsculas para evitar duplicados lógicos.</summary>
    private static ProductoInventarioDto ToDto(Producto p) =>
        new() { Id = p.Id, Nombre = p.Nombre, Cantidad = p.Cantidad };
}
