using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers;

// [Authorize] — pendiente de integración con middleware JWT
[Route("api/billetera")]
[ApiController]
public class BilleteraController : ControllerBase
{
    private readonly IBilleteraService _billeteraService;
    private readonly IBilleteraConsultaService _consultaService;

    public BilleteraController(
        IBilleteraService billeteraService,
        IBilleteraConsultaService consultaService)
    {
        _billeteraService = billeteraService;
        _consultaService = consultaService;
    }

    // GET api/billetera
    [HttpGet]
    public async Task<IActionResult> GetResumen(
        [FromHeader(Name = "X-Session-Token")] string tokenSesion)
    {
        if (string.IsNullOrWhiteSpace(tokenSesion))
            return Unauthorized(new { mensaje = "No se envió el token de sesión." });

        // Reutiliza el servicio existente; resuelve usuarioId desde la sesión
        var sesionRepo = HttpContext.RequestServices.GetRequiredService<ISesionUsuarioRepository>();
        var sesion = await sesionRepo.ObtenerSesionActivaAsync(tokenSesion);
        if (sesion == null) return Unauthorized(new { mensaje = "Sesión inválida o expirada." });

        var resumen = await _billeteraService.GetBilletera(sesion.UsuarioId);
        return Ok(resumen);
    }

    // GET api/billetera/saldo/{monedaId}
    [HttpGet("saldo/{monedaId:int}")]
    public async Task<IActionResult> GetSaldoMoneda(
        [FromHeader(Name = "X-Session-Token")] string tokenSesion,
        int monedaId)
    {
        var resultado = await _consultaService.GetSaldoMonedaAsync(tokenSesion, monedaId);
        if (!resultado.Exito)
            return Unauthorized(new { mensaje = resultado.Mensaje });
        return Ok(resultado.Data);
    }

    // GET api/billetera/movimientos?monedaId=&tipoMovimiento=&desde=&hasta=&pagina=1&tamano=20
    [HttpGet("movimientos")]
    public async Task<IActionResult> GetMovimientos(
        [FromHeader(Name = "X-Session-Token")] string tokenSesion,
        [FromQuery] int? monedaId,
        [FromQuery] string? tipoMovimiento,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20)
    {
        if (pagina < 1) pagina = 1;
        if (tamano < 1 || tamano > 100) tamano = 20;

        var resultado = await _consultaService.GetMovimientosPaginadosAsync(
            tokenSesion, monedaId, tipoMovimiento, desde, hasta, pagina, tamano);

        if (!resultado.Exito)
            return Unauthorized(new { mensaje = resultado.Mensaje });
        return Ok(resultado.Data);
    }
}
