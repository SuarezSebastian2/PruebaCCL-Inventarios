using CclInventario.Api.Options;
using Microsoft.Extensions.Options;

namespace CclInventario.Api.Services;

/// <summary>
/// Valida credenciales contra la lista configurada (equivalente a credenciales fijas en memoria).
/// </summary>
public class DemoUserStore
{
    private readonly HashSet<(string User, string Pass)> _users;

    public DemoUserStore(IOptions<DemoAuthOptions> options)
    {
        _users = new HashSet<(string, string)>(
            options.Value.Users.Select(u => (u.Usuario.Trim(), u.Clave)),
            new UserComparer());
    }

    public bool Validate(string usuario, string clave) =>
        _users.Contains((usuario.Trim(), clave));

    private sealed class UserComparer : IEqualityComparer<(string User, string Pass)>
    {
        public bool Equals((string User, string Pass) x, (string User, string Pass) y) =>
            string.Equals(x.User, y.User, StringComparison.Ordinal)
            && string.Equals(x.Pass, y.Pass, StringComparison.Ordinal);

        public int GetHashCode((string User, string Pass) obj) =>
            HashCode.Combine(obj.User, obj.Pass);
    }
}
