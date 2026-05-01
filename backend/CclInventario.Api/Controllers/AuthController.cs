using CclInventario.Api.Dtos;
using CclInventario.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CclInventario.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly DemoUserStore _users;
    private readonly JwtTokenBuilder _jwt;

    public AuthController(DemoUserStore users, JwtTokenBuilder jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    /// <summary>Autenticación con credenciales de demostración; devuelve JWT Bearer.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        var (token, expiresIn) = _jwt.CreateToken(request.Usuario);
        return Ok(new LoginResponse
        {
            Token = token,
            ExpiresInSeconds = expiresIn,
            TokenType = "Bearer"
        });
    }
}
