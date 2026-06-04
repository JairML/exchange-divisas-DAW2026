using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/monedas")]
public class MonedasController : ControllerBase
{
    private readonly IMonedasService _service;
    public MonedasController(IMonedasService service) => _service = service;

    // GET api/monedas?tipo=Internacional&activa=true
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? tipo,
        [FromQuery] bool? activa)
    {
        var result = await _service.ObtenerTodosAsync(tipo, activa);
        return Ok(result);
    }

    // GET api/monedas/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.ObtenerPorIdAsync(id);
        return result is null
            ? NotFound(new { mensaje = "Moneda no encontrada." })
            : Ok(result);
    }
}
