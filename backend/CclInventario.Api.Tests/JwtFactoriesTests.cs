using System.IdentityModel.Tokens.Jwt;
using CclInventario.Api.Patterns.Factory;
using Microsoft.IdentityModel.Tokens;

namespace CclInventario.Api.Tests;

public class JwtFactoriesTests
{
    [Fact]
    public void JwtTokenFactory_emits_parseable_token_with_subject()
    {
        var factory = new JwtTokenFactory(TestJwtOptions.CreateValidOptions());

        var (token, expiresIn) = factory.CreateToken("admin");

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(expiresIn > 0);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        Assert.Equal("admin", jwt.Subject);
    }

    [Fact]
    public void JwtBearerConfigurationFactory_builds_validation_parameters()
    {
        var factory = new JwtBearerConfigurationFactory(TestJwtOptions.CreateValidOptions());

        var parameters = factory.CreateValidationParameters();

        Assert.True(parameters.ValidateIssuer);
        Assert.NotNull(parameters.IssuerSigningKey);
        var symmetricKey = Assert.IsType<SymmetricSecurityKey>(parameters.IssuerSigningKey);

        var signingKeyBytes = symmetricKey.Key;
        Assert.True(signingKeyBytes.Length >= 32);
    }
}
