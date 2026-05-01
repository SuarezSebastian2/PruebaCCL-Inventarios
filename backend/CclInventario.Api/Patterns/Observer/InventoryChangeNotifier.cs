namespace CclInventario.Api.Patterns.Observer;

/// <summary>
/// GoF Subject concreto: despacha el evento a todos los <see cref="IInventoryObserver"/> (DI).
/// </summary>
public sealed class InventoryChangeNotifier : IInventoryChangeNotifier
{
    private readonly IReadOnlyList<IInventoryObserver> _observers;

    public InventoryChangeNotifier(IEnumerable<IInventoryObserver> observers) =>
        _observers = observers.ToList();

    public async Task NotifyMovimientoRegistradoAsync(InventoryMovimientoEvent evt, CancellationToken ct)
    {
        foreach (var observer in _observers)
        {
            await observer.OnMovimientoRegistradoAsync(evt, ct).ConfigureAwait(false);
        }
    }
}
