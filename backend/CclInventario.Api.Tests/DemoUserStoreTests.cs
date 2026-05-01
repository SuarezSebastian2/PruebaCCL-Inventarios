using CclInventario.Api.Options;
using CclInventario.Api.Services;
using Microsoft.Extensions.Options;

namespace CclInventario.Api.Tests;

public class DemoUserStoreTests
{
    [Fact]
    public void Validate_returns_true_for_configured_user()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new DemoAuthOptions
        {
            Users =
            [
                new DemoUser { Usuario = "admin", Clave = "secret" }
            ]
        });
        IDemoUserStore store = new DemoUserStore(options);

        Assert.True(store.Validate("admin", "secret"));
    }

    [Fact]
    public void Validate_returns_false_for_bad_password()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new DemoAuthOptions
        {
            Users = [new DemoUser { Usuario = "admin", Clave = "secret" }]
        });
        IDemoUserStore store = new DemoUserStore(options);

        Assert.False(store.Validate("admin", "wrong"));
    }
}
