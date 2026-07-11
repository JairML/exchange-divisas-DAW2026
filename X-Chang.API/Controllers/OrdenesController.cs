using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.DTOs;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/ordenes")]
[Authorize]
public class OrdenesController : ControllerBase
{
    private readonly IOrdenService _ordenService;
    private readonly INotificacionesCorreoService _notifService;

    public OrdenesController(IOrdenService ordenService, INotificacionesCorreoService notifService)
    {
        _ordenService = ordenService;
        _notifService = notifService;
    }

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerOrden(int id)
    {
        var orden = await _ordenService.ObtenerOrdenPorIdAsync(UsuarioId, id);
        if (orden == null) return NotFound(new { error = "Orden no encontrada." });
        return Ok(orden);
    }

    [HttpGet]
    public async Task<IActionResult> ListarOrdenes([FromQuery] FiltroOrdenesRequest filtro)
    {
        var resultado = await _ordenService.ListarOrdenesActivasAsync(UsuarioId, filtro);
        return Ok(resultado);
    }

    [HttpGet("libro/{parMonedaId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerLibroOrdenes(int parMonedaId)
    {
        var resultado = await _ordenService.ObtenerLibroOrdenesAsync(parMonedaId);
        return Ok(resultado);
    }

    [HttpGet("libro/{parMonedaId:int}/detalle")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerLibroOrdenesDetalle(int parMonedaId, [FromQuery] int limite = 10)
    {
        var resultado = await _ordenService.ObtenerLibroOrdenesDetalleAsync(parMonedaId, limite);
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenRequest request)
    {
        try
        {
            var result = await _ordenService.CrearOrdenCompraAsync(UsuarioId, request);

            await _notifService.EncolarAsync(
                UsuarioId,
                "OrdenCompra",
                $"Orden de compra registrada: {result.MonedaOrigen} → {result.MonedaDestino}",
                $"Tu orden de compra de {result.CantidadOriginal} {result.MonedaDestino} a {result.PrecioUnitario} fue registrada. " +
                $"Ejecutado hasta ahora: {result.CantidadObtenida} {result.MonedaDestino}. " +
                $"Estado: {result.Estado}. Fecha: {result.FechaCreacion:dd/MM/yyyy HH:mm}.",
                "OrdenCompra",
                result.OrdenCompraId);

            return CreatedAtAction(nameof(ObtenerOrden), new { id = result.OrdenCompraId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
