using CclInventario.Api.Dtos;
using Microsoft.AspNetCore.Http;

namespace CclInventario.Api.Patterns.Facade;

/// <summary>Resultado homogéneo para alta, baja y edición de productos (sin movimientos).</summary>
public sealed record InventoryFacadeCrudResult(
    bool Exito,
    ProductoInventarioDto? Valor,
    int CodigoHttp,
    object? CuerpoError)
{
    public static InventoryFacadeCrudResult Created(ProductoInventarioDto dto) =>
        new(true, dto, StatusCodes.Status201Created, null);

    public static InventoryFacadeCrudResult Ok(ProductoInventarioDto dto) =>
        new(true, dto, StatusCodes.Status200OK, null);

    public static InventoryFacadeCrudResult Deleted() =>
        new(true, null, StatusCodes.Status204NoContent, null);

    public static InventoryFacadeCrudResult BadRequest(object body) =>
        new(false, null, StatusCodes.Status400BadRequest, body);

    public static InventoryFacadeCrudResult NotFound(object body) =>
        new(false, null, StatusCodes.Status404NotFound, body);

    public static InventoryFacadeCrudResult Conflict(object body) =>
        new(false, null, StatusCodes.Status409Conflict, body);
}
