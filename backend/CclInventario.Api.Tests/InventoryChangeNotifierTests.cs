using CclInventario.Api.Dtos;
using CclInventario.Api.Patterns.Observer;
using Moq;

namespace CclInventario.Api.Tests;

public class InventoryChangeNotifierTests
{
    [Fact]
    public async Task Notify_invokes_all_observers()
    {
        var first = new Mock<IInventoryObserver>();
        var second = new Mock<IInventoryObserver>();
        var notifier = new InventoryChangeNotifier(new[] { first.Object, second.Object });
        var evt = new InventoryMovimientoEvent
        {
            Estado = new ProductoInventarioDto { Id = 1, Nombre = "X", Cantidad = 1 },
            TipoMovimiento = "entrada",
            Cantidad = 1,
            OperacionSecuencial = 1
        };

        await notifier.NotifyMovimientoRegistradoAsync(evt, CancellationToken.None);

        first.Verify(o => o.OnMovimientoRegistradoAsync(evt, It.IsAny<CancellationToken>()), Times.Once);
        second.Verify(o => o.OnMovimientoRegistradoAsync(evt, It.IsAny<CancellationToken>()), Times.Once);
    }
}
