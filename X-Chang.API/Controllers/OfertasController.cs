using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.DTOs;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/ofertas")]
[Authorize]
public class OfertasController : ControllerBase
{
    private readonly IOfertaService _ofertaService;
    private readonly INotificacionesCorreoService _notifService;

    public OfertasController(IOfertaService ofertaService, INotificacionesCorreoService notifService)
    {
        _ofertaService = ofertaService;
        _notifService = notifService;
    }

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> ListarOfertas([FromQuery] FiltroOfertasRequest filtro)
    {
        var resultado = await _ofertaService.ListarOfertasActivasAsync(UsuarioId, filtro);
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> CrearOferta([FromBody] CrearOfertaRequest request)
    {
        try
        {
            var result = await _ofertaService.CrearOfertaVentaAsync(UsuarioId, request);

            await _notifService.EncolarAsync(
                UsuarioId,
                "OfertaVenta",
                $"Oferta de venta registrada: {result.MonedaOrigen} → {result.MonedaDestino}",
                $"Tu oferta de venta de {result.CantidadOriginal} {result.MonedaOrigen} a {result.PrecioUnitario} fue registrada. " +
                $"Ejecutado hasta ahora: {result.CantidadVendida} {result.MonedaOrigen}. " +
                $"Estado: {result.Estado}. Fecha: {result.FechaCreacion:dd/MM/yyyy HH:mm}.",
                "OfertaVenta",
                result.OfertaVentaId);

            return Ok(result);
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
