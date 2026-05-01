namespace CclInventario.Api.Patterns.Observer;

/// <summary>GoF Observer: reacciona a movimientos de inventario confirmados.</summary>
public interface IInventoryObserver
{
    Task OnMovimientoRegistradoAsync(InventoryMovimientoEvent evt, CancellationToken ct);
}
