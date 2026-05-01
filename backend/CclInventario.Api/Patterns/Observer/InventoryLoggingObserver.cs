using Microsoft.Extensions.Logging;

namespace CclInventario.Api.Patterns.Observer;

/// <summary>GoF ConcreteObserver: traza de negocio sin acoplar el dominio a detalles de logging.</summary>
public sealed class InventoryLoggingObserver : IInventoryObserver
{
    private readonly ILogger<InventoryLoggingObserver> _logger;

    public InventoryLoggingObserver(ILogger<InventoryLoggingObserver> logger) =>
        _logger = logger;

    public Task OnMovimientoRegistradoAsync(InventoryMovimientoEvent evt, CancellationToken ct)
    {
        _logger.LogInformation(
            "Movimiento #{Op}: tipo={Tipo} cantidad={Cantidad} producto={ProductoId} stock={Stock}",
            evt.OperacionSecuencial,
            evt.TipoMovimiento,
            evt.Cantidad,
            evt.Estado.Id,
            evt.Estado.Cantidad);
        return Task.CompletedTask;
    }
}
