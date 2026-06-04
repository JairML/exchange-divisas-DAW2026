using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/pares-moneda")]
public class ParesMonedaController : ControllerBase
{
    private readonly IParesMonedaService _service;
    public ParesMonedaController(IParesMonedaService service) => _service = service;

    // GET api/pares-moneda?activo=true
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activo)
    {
        var result = await _service.ObtenerTodosAsync(activo);
        return Ok(result);
    }

    // GET api/pares-moneda/{id}  — detalle + libro de órdenes
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.ObtenerDetalleAsync(id);
        return result is null
            ? NotFound(new { mensaje = "Par de monedas no encontrado." })
            : Ok(result);
    }
}
