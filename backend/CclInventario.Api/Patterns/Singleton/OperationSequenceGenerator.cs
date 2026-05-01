namespace CclInventario.Api.Patterns.Singleton;

/// <summary>
/// GoF Singleton (una instancia en el contenedor DI): generador de secuencia thread-safe para correlacionar operaciones.
/// </summary>
public sealed class OperationSequenceGenerator
{
    private long _current;

    public long Next() => Interlocked.Increment(ref _current);
}
