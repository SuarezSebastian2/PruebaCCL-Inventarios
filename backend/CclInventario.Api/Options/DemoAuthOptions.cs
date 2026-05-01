namespace CclInventario.Api.Options;

/// <summary>
/// Credenciales de demostración en memoria (configuración).
/// </summary>
public class DemoAuthOptions
{
    public const string SectionName = "Auth";

    public List<DemoUser> Users { get; set; } = new();
}

public class DemoUser
{
    public string Usuario { get; set; } = string.Empty;

    public string Clave { get; set; } = string.Empty;
}
