using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.API.Helpers;
using X_Chang.CORE.Core.DTOs.Ofertas;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers;

// US-004: visualización paginada de ofertas de venta activas del usuario.
[ApiController]
[Route("api/ofertas")]
[Authorize]
public class OfertasController : ControllerBase
{
    private readonly IOfertaService _ofertaService;

    public OfertasController(IOfertaService ofertaService) => _ofertaService = ofertaService;

    [HttpGet]
    public async Task<IActionResult> GetMisOfertas([FromQuery] FiltroOfertasDto filtro)
    {
        var usuarioId = this.GetUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var result = await _ofertaService.ObtenerMisOfertasAsync(usuarioId.Value, filtro);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOferta(int id)
    {
        var usuarioId = this.GetUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var result = await _ofertaService.ObtenerOfertaAsync(usuarioId.Value, id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
