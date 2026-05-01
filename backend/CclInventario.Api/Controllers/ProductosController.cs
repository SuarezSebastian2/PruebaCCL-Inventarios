using CclInventario.Api.Dtos;
using CclInventario.Api.Patterns.Facade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CclInventario.Api.Controllers;

[ApiController]
[Route("productos")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IInventoryFacade _inventory;

    public ProductosController(IInventoryFacade inventory) => _inventory = inventory;

    /// <summary>Lista el inventario actual (id, nombre, cantidad).</summary>
    [HttpGet("inventario")]
    [ProducesResponseType(typeof(IEnumerable<ProductoInventarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductoInventarioDto>>> Inventario(CancellationToken ct)
    {
        var list = await _inventory.ListInventarioAsync(ct).ConfigureAwait(false);
        return Ok(list);
    }

    /// <summary>Registra una entrada o salida de inventario.</summary>
    [HttpPost("movimiento")]
    [ProducesResponseType(typeof(ProductoInventarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductoInventarioDto>> Movimiento(
        [FromBody] MovimientoRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _inventory.RegistrarMovimientoAsync(request, ct).ConfigureAwait(false);
        if (!result.Exito)
        {
            return StatusCode(result.CodigoHttp, result.CuerpoError);
        }

        return Ok(result.Valor);
    }
}
