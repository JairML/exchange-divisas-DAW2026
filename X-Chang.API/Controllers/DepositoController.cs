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
    public class DepositoController : ControllerBase
    {
        private readonly IDepositoService _depositoService;
        private readonly INotificacionesCorreoService _notifService;

        public DepositoController(IDepositoService depositoService, INotificacionesCorreoService notifService)
        {
            _depositoService = depositoService;
            _notifService = notifService;
        }

        private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("metodos-pago")]
        public async Task<IActionResult> GetMetodosPago()
        {
            var metodos = await _depositoService.GetMetodosPago(UsuarioId);
            return Ok(metodos);
        }

        [HttpPost("calcular")]
        public async Task<IActionResult> Calcular([FromBody] DepositoCalcularDTO dto)
        {
            var resultado = await _depositoService.Calcular(UsuarioId, dto);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(resultado.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] DepositoCreateDTO dto)
        {
            var resultado = await _depositoService.RegistrarDeposito(UsuarioId, dto);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            var dep = resultado.Data!;
            var cuerpoHtml = EmailHtmlBuilder.Build(
                "Confirmación de depósito",
                "Tu depósito fue registrado exitosamente. A continuación encontrarás el resumen de la operación:",
                [
                    ("Monto depositado",      $"{dep.MontoDepositado.ToString("N2")} {dep.CodigoISO}"),
                    ("Comisión aplicada",     $"{dep.ComisionAplicada.ToString("N2")} {dep.CodigoISO}"),
                    ("Total pagado",          $"{dep.TotalPagado.ToString("N2")} {dep.CodigoISO}"),
                    ("Nuevo saldo disponible",$"{dep.NuevoSaldo.ToString("N2")} {dep.CodigoISO}"),
                    ("Fecha y hora",          dep.FechaDeposito.ToString("dd/MM/yyyy HH:mm")),
                ]);

            await _notifService.EncolarAsync(
                UsuarioId,
                "Deposito",
                $"Depósito de {dep.CodigoISO} completado",
                cuerpoHtml,
                "Deposito",
                dep.DepositoId);

            return Ok(dep);
        }
    }
}
