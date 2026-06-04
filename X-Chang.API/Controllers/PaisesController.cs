using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/paises")]
public class PaisesController : ControllerBase
{
    private readonly IPaisesService _service;
    public PaisesController(IPaisesService service) => _service = service;

    // GET api/paises
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.ObtenerTodosAsync();
        return Ok(result);
    }

    // GET api/paises/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.ObtenerPorIdAsync(id);
        return result is null
            ? NotFound(new { mensaje = "País no encontrado." })
            : Ok(result);
    }
}
