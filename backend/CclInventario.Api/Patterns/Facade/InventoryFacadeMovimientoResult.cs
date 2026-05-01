using CclInventario.Api.Dtos;
using Microsoft.AspNetCore.Http;

namespace CclInventario.Api.Patterns.Facade;

public sealed record InventoryFacadeMovimientoResult(
    bool Exito,
    ProductoInventarioDto? Valor,
    int CodigoHttp,
    object? CuerpoError)
{
    public static InventoryFacadeMovimientoResult Success(ProductoInventarioDto dto) =>
        new(true, dto, StatusCodes.Status200OK, null);

    public static InventoryFacadeMovimientoResult BadRequest(object body) =>
        new(false, null, StatusCodes.Status400BadRequest, body);

    public static InventoryFacadeMovimientoResult NotFound(object body) =>
        new(false, null, StatusCodes.Status404NotFound, body);

    public static InventoryFacadeMovimientoResult Conflict(object body) =>
        new(false, null, StatusCodes.Status409Conflict, body);
}
