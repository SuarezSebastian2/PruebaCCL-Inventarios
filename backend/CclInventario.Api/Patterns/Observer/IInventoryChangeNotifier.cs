namespace CclInventario.Api.Patterns.Observer;

/// <summary>GoF Subject: notifica a observadores registrados.</summary>
public interface IInventoryChangeNotifier
{
    Task NotifyMovimientoRegistradoAsync(InventoryMovimientoEvent evt, CancellationToken ct);
}
