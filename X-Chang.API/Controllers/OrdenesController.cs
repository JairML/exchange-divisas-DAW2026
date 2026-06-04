using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/ordenes")]
public class OrdenesController : ControllerBase
{
    private readonly IOrdenService _ordenService;

    public OrdenesController(IOrdenService ordenService)
    {
        _ordenService = ordenService;
    }

    [HttpGet("libro/{parMonedaId:int}")]
    public async Task<IActionResult> ObtenerLibroOrdenes(int parMonedaId)
    {
        var result = await _ordenService.ObtenerLibroOrdenesAsync(parMonedaId);
        return Ok(result);
    }

    [HttpGet("libro/{parMonedaId:int}/detalle")]
    public async Task<IActionResult> ObtenerLibroOrdenesDetalle(int parMonedaId, [FromQuery] int limite = 10)
    {
        var result = await _ordenService.ObtenerLibroOrdenesDetalleAsync(parMonedaId, limite);
        return Ok(result);
    }
}