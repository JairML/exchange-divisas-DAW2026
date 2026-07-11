using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using X_Chang.API.Helpers;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RetiroController : ControllerBase
    {
        private readonly IRetiroService _retiroService;
        private readonly INotificacionesCorreoService _notifService;

        public RetiroController(IRetiroService retiroService, INotificacionesCorreoService notifService)
        {
            _retiroService = retiroService;
            _notifService = notifService;
        }

        private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("metodos-cobro")]
        public async Task<IActionResult> GetMetodosCobro()
        {
            var metodos = await _retiroService.GetMetodosCobro(UsuarioId);
            return Ok(metodos);
        }

        [HttpPost("calcular")]
        public async Task<IActionResult> Calcular([FromBody] RetiroCalcularDTO dto)
        {
            var resultado = await _retiroService.Calcular(UsuarioId, dto);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(resultado.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] RetiroCreateDTO dto)
        {
            var resultado = await _retiroService.RegistrarRetiro(UsuarioId, dto);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            var ret = resultado.Data!;
            var cuerpoHtml = $@"<!DOCTYPE html>
<html>
<body style=""margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;"">
  <div style=""max-width:600px;margin:32px auto;background:white;border-radius:12px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.1);"">
    <div style=""background:linear-gradient(135deg,#1a237e,#1565c0);padding:36px;text-align:center;"">
      <h1 style=""color:white;margin:0;font-size:26px;letter-spacing:1px;"">💱 X-Chang</h1>
      <p style=""color:rgba(255,255,255,0.8);margin:8px 0 0;"">Notificación de transacción</p>
    </div>
    <div style=""padding:36px;"">
      <h2 style=""color:#1565c0;margin-top:0;"">Retiro registrado</h2>
      <table style=""width:100%;border-collapse:collapse;"">
        <tr style=""background:#f5f5f5;"">
          <td style=""padding:12px;font-weight:bold;"">Monto retirado</td>
          <td style=""padding:12px;"">{ret.MontoRetirado.ToString("N2")} {ret.CodigoISO}</td>
        </tr>
        <tr>
          <td style=""padding:12px;font-weight:bold;"">Comisión aplicada</td>
          <td style=""padding:12px;"">{ret.ComisionAplicada.ToString("N2")} {ret.CodigoISO}</td>
        </tr>
        <tr style=""background:#f5f5f5;"">
          <td style=""padding:12px;font-weight:bold;"">Monto final a recibir</td>
          <td style=""padding:12px;"">{ret.MontoFinalRecibido.ToString("N2")} {ret.CodigoISO}</td>
        </tr>
        <tr>
          <td style=""padding:12px;font-weight:bold;"">Nuevo saldo</td>
          <td style=""padding:12px;"">{ret.NuevoSaldo.ToString("N2")} {ret.CodigoISO}</td>
        </tr>
        <tr style=""background:#f5f5f5;"">
          <td style=""padding:12px;font-weight:bold;"">Fecha</td>
          <td style=""padding:12px;"">{ret.FechaRetiro.ToString("dd/MM/yyyy HH:mm")}</td>
        </tr>
      </table>
    </div>
    <div style=""background:#1a237e;padding:20px;text-align:center;"">
      <p style=""color:rgba(255,255,255,0.7);margin:0;font-size:13px;"">El equipo de X-Chang</p>
    </div>
  </div>
</body>
</html>";

            await _notifService.EncolarAsync(
                UsuarioId,
                "Retiro",
                $"Retiro de {ret.CodigoISO} completado",
                cuerpoHtml,
                "Retiro",
                ret.RetiroId);

            return Ok(ret);
        }
    }
}
