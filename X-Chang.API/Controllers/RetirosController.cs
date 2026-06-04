using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.DTOs.Billetera;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers;

// [Authorize] — pendiente de integración con middleware JWT
[ApiController]
[Route("api/retiros")]
public class RetirosController : ControllerBase
{
    private readonly IRetirosService _service;
    public RetirosController(IRetirosService service) => _service = service;

    // GET api/retiros?pagina=1&tamano=20
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromHeader(Name = "X-Session-Token")] string tokenSesion,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20)
    {
        if (pagina < 1) pagina = 1;
        if (tamano < 1 || tamano > 100) tamano = 20;

        var resultado = await _service.ListarAsync(tokenSesion, pagina, tamano);
        if (!resultado.Exito)
            return Unauthorized(new { mensaje = resultado.Mensaje });

        var (items, total) = resultado.Data!;
        return Ok(new { items, total, pagina, tamano });
    }

    // GET api/retiros/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detalle(
        [FromHeader(Name = "X-Session-Token")] string tokenSesion,
        int id)
    {
        var resultado = await _service.ObtenerDetalleAsync(tokenSesion, id);
        if (!resultado.Exito)
            return resultado.Mensaje!.Contains("Sesión")
                ? Unauthorized(new { mensaje = resultado.Mensaje })
                : NotFound(new { mensaje = resultado.Mensaje });
        return Ok(resultado.Data);
    }

    // POST api/retiros
    // Body: { monedaId, metodoPagoId, montoRetirado, voucherUrl? }
    [HttpPost]
    public async Task<IActionResult> Retirar(
        [FromHeader(Name = "X-Session-Token")] string tokenSesion,
        [FromBody] RetirarDto dto)
    {
        try
        {
            var resultado = await _service.RetirarAsync(tokenSesion, dto);
            if (!resultado.Exito)
                return resultado.Mensaje!.Contains("Sesión")
                    ? Unauthorized(new { mensaje = resultado.Mensaje })
                    : BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(resultado.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
