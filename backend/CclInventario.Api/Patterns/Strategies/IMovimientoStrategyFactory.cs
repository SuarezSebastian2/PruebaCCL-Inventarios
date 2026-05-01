namespace CclInventario.Api.Patterns.Strategies;

/// <summary>GoF Factory: selecciona la estrategia de movimiento según el tipo solicitado.</summary>
public interface IMovimientoStrategyFactory
{
    bool TryGetStrategy(string tipo, out IMovimientoStrategy? strategy);
}
