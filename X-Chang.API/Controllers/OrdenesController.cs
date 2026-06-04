using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.API.Helpers;
using X_Chang.CORE.Core.DTOs.Ordenes;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers;

// US-004: visualización paginada de órdenes de compra activas del usuario.
[ApiController]
[Route("api/ordenes")]
[Authorize]
public class OrdenesController : ControllerBase
{
    private readonly IOrdenService _ordenService;

    public OrdenesController(IOrdenService ordenService) => _ordenService = ordenService;

    [HttpGet]
    public async Task<IActionResult> GetMisOrdenes([FromQuery] FiltroOrdenesDto filtro)
    {
        var usuarioId = this.GetUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var result = await _ordenService.ObtenerMisOrdenesAsync(usuarioId.Value, filtro);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrden(int id)
    {
        var usuarioId = this.GetUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var result = await _ordenService.ObtenerOrdenAsync(usuarioId.Value, id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
