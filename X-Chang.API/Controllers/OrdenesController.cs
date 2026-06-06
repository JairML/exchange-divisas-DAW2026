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

    public OrdenesController(IOrdenService ordenService)
    {
        _ordenService = ordenService;
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
