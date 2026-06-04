using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/metodos-pago")]
public class MetodosPagoController : ControllerBase
{
    private readonly IMetodosPagoService _service;
    public MetodosPagoController(IMetodosPagoService service) => _service = service;

    // GET api/metodos-pago
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.ObtenerTodosAsync();
        return Ok(result);
    }

    // GET api/metodos-pago/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.ObtenerPorIdAsync(id);
        return result is null
            ? NotFound(new { mensaje = "Método de pago no encontrado." })
            : Ok(result);
    }

    // GET api/metodos-pago/deposito  — Tipo IN ('Pago','Ambos'), país del usuario
    [HttpGet("deposito")]
    public async Task<IActionResult> ParaDeposito(
        [FromHeader(Name = "X-Session-Token")] string tokenSesion)
    {
        try
        {
            var resultado = await _service.ObtenerParaDepositoAsync(tokenSesion);
            if (!resultado.Exito)
                return Unauthorized(new { mensaje = resultado.Mensaje });
            return Ok(resultado.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // GET api/metodos-pago/retiro  — Tipo IN ('Cobro','Ambos'), país del usuario
    [HttpGet("retiro")]
    public async Task<IActionResult> ParaRetiro(
        [FromHeader(Name = "X-Session-Token")] string tokenSesion)
    {
        try
        {
            var resultado = await _service.ObtenerParaRetiroAsync(tokenSesion);
            if (!resultado.Exito)
                return Unauthorized(new { mensaje = resultado.Mensaje });
            return Ok(resultado.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
