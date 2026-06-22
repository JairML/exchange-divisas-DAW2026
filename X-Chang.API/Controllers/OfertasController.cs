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

    public OfertasController(IOfertaService ofertaService)
    {
        _ofertaService = ofertaService;
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
