using CclInventario.Api.Dtos;

namespace CclInventario.Api.Patterns.Facade;

/// <summary>
/// GoF Facade: operaciones de alto nivel sobre inventario sin exponer DbContext ni estrategias a los controladores.
/// </summary>
public interface IInventoryFacade
{
    Task<IReadOnlyList<ProductoInventarioDto>> ListInventarioAsync(CancellationToken ct);

    Task<InventoryFacadeMovimientoResult> RegistrarMovimientoAsync(MovimientoRequest request, CancellationToken ct);

    Task<ProductoInventarioDto?> GetProductoByIdAsync(int id, CancellationToken ct);

    Task<InventoryFacadeCrudResult> CrearProductoAsync(CreateProductoRequest request, CancellationToken ct);

    Task<InventoryFacadeCrudResult> ActualizarProductoAsync(int id, UpdateProductoRequest request, CancellationToken ct);

    Task<InventoryFacadeCrudResult> EliminarProductoAsync(int id, CancellationToken ct);
}
