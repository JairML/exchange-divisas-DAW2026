using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CancelacionController : ControllerBase
    {
        private readonly ICancelacionService _cancelacionService;
        private readonly INotificacionesCorreoService _notifService;

        public CancelacionController(ICancelacionService cancelacionService, INotificacionesCorreoService notifService)
        {
            _cancelacionService = cancelacionService;
            _notifService = notifService;
        }

        private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("detalle")]
        public async Task<IActionResult> GetDetalle([FromQuery] string tipo, [FromQuery] int id)
        {
            var resultado = await _cancelacionService.GetDetalle(UsuarioId, tipo, id);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });
            return Ok(resultado.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Cancelar([FromBody] CancelacionConfirmarDTO dto)
        {
            var resultado = await _cancelacionService.Cancelar(UsuarioId, dto);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            var can = resultado.Data!;
            await _notifService.EncolarAsync(
                UsuarioId,
                "Cancelacion",
                $"Cancelación de {can.TipoOperacion} completada",
                EmailHtmlBuilder.Build(
                    $"Cancelación de {can.TipoOperacion}",
                    $"Tu {can.TipoOperacion} del par {can.Par} fue cancelada exitosamente.",
                    [
                        ("Tipo de operación",  can.TipoOperacion),
                        ("Par",                can.Par),
                        ("Cantidad cancelada", can.CantidadCancelada.ToString("N2")),
                        ("Monto reembolsado",  $"{can.MontoReembolsado.ToString("N2")} {can.MonedaReembolso}"),
                        ("Nuevo saldo",        $"{can.NuevoSaldo.ToString("N2")} {can.MonedaReembolso}"),
                        ("Fecha y hora",       can.FechaCancelacion.ToString("dd/MM/yyyy HH:mm")),
                    ]),
                can.TipoOperacion == "Orden de compra" ? "OrdenCompra" : "OfertaVenta",
                can.CancelacionId);

            return Ok(can);
        }
    }
}
