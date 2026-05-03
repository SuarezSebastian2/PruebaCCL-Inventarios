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

    /// <summary>Obtiene un producto por id (para edición).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductoInventarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoInventarioDto>> ObtenerPorId(int id, CancellationToken ct)
    {
        var p = await _inventory.GetProductoByIdAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return NotFound(new { message = $"No existe producto con id {id}." });
        }

        return Ok(p);
    }

    /// <summary>Alta de producto con stock inicial (sin cambiar esquema de tabla).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductoInventarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductoInventarioDto>> Crear(
        [FromBody] CreateProductoRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _inventory.CrearProductoAsync(request, ct).ConfigureAwait(false);
        if (!result.Exito)
        {
            return StatusCode(result.CodigoHttp, result.CuerpoError);
        }

        return CreatedAtAction(nameof(ObtenerPorId), new { id = result.Valor!.Id }, result.Valor);
    }

    /// <summary>Actualiza solo el nombre del producto; la cantidad se ajusta con movimientos.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductoInventarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductoInventarioDto>> Actualizar(
        int id,
        [FromBody] UpdateProductoRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _inventory.ActualizarProductoAsync(id, request, ct).ConfigureAwait(false);
        if (!result.Exito)
        {
            return StatusCode(result.CodigoHttp, result.CuerpoError);
        }

        return Ok(result.Valor);
    }

    /// <summary>Elimina el producto solo si la cantidad en stock es 0.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        var result = await _inventory.EliminarProductoAsync(id, ct).ConfigureAwait(false);
        if (!result.Exito)
        {
            return StatusCode(result.CodigoHttp, result.CuerpoError);
        }

        return NoContent();
    }
}
