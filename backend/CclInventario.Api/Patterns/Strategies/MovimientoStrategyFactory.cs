namespace CclInventario.Api.Patterns.Strategies;

/// <summary>
/// GoF Abstract Factory / Simple Factory sobre estrategias concretas (registradas vía DI).
/// </summary>
public sealed class MovimientoStrategyFactory : IMovimientoStrategyFactory
{
    private readonly IReadOnlyDictionary<string, IMovimientoStrategy> _strategies;

    public MovimientoStrategyFactory(IEnumerable<IMovimientoStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(
            s => s.TipoNormalizado,
            StringComparer.OrdinalIgnoreCase);
    }

    public bool TryGetStrategy(string tipo, out IMovimientoStrategy? strategy)
    {
        var key = tipo.Trim().ToLowerInvariant();
        return _strategies.TryGetValue(key, out strategy);
    }
}
