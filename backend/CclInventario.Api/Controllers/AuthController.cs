using CclInventario.Api.Dtos;
using CclInventario.Api.Patterns.Factory;
using CclInventario.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CclInventario.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IDemoUserStore _users;
    private readonly IJwtTokenFactory _tokenFactory;

    public AuthController(IDemoUserStore users, IJwtTokenFactory tokenFactory)
    {
        _users = users;
        _tokenFactory = tokenFactory;
    }

    /// <summary>Autenticación con credenciales de demostración; devuelve JWT Bearer.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!_users.Validate(request.Usuario, request.Clave))
        {
            return Unauthorized(new { message = "Usuario o clave incorrectos." });
        }

        var (token, expiresIn) = _tokenFactory.CreateToken(request.Usuario);
        return Ok(new LoginResponse
        {
            Token = token,
            ExpiresInSeconds = expiresIn,
            TokenType = "Bearer"
        });
    }
}
