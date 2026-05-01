namespace CclInventario.Api.Services;

public interface IDemoUserStore
{
    bool Validate(string usuario, string clave);
}
