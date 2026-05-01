using CclInventario.Api.Data;
using CclInventario.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CclInventario.Api.Controllers;

[ApiController]
[Route("productos")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductosController(AppDbContext db) => _db = db;

    /// <summary>Lista el inventario actual (id, nombre, cantidad).</summary>
    [HttpGet("inventario")]
    [ProducesResponseType(typeof(IEnumerable<ProductoInventarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductoInventarioDto>>> Inventario(CancellationToken ct)
    {
        var list = await _db.Productos
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new ProductoInventarioDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Cantidad = p.Cantidad
            })
            .ToListAsync(ct);

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

        var tipo = request.Tipo.Trim().ToLowerInvariant();
        if (tipo is not ("entrada" or "salida"))
        {
            return BadRequest(new { message = "Tipo debe ser 'entrada' o 'salida'." });
        }

        var producto = await _db.Productos.FirstOrDefaultAsync(p => p.Id == request.ProductoId, ct);
        if (producto is null)
        {
            return NotFound(new { message = $"No existe producto con id {request.ProductoId}." });
        }

        if (tipo == "entrada")
        {
            producto.Cantidad += request.Cantidad;
        }
        else
        {
            if (producto.Cantidad < request.Cantidad)
            {
                return Conflict(new
                {
                    message = "Stock insuficiente para la salida solicitada.",
                    disponible = producto.Cantidad
                });
            }

            producto.Cantidad -= request.Cantidad;
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new ProductoInventarioDto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Cantidad = producto.Cantidad
        });
    }
}
