using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using X_Chang.API.Helpers;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    // US-022: Cancelación de orden u oferta.
    // [Authorize] se habilitará cuando esté integrado el login (US-001/US-002).
    [Route("api/[controller]")]
    [ApiController]
    public class CancelacionController : ControllerBase
    {
        private readonly ICancelacionService _cancelacionService;
        private readonly INotificacionesCorreoService _notifService;

        public CancelacionController(
            ICancelacionService cancelacionService,
            INotificacionesCorreoService notifService)
        {
            _cancelacionService = cancelacionService;
            _notifService = notifService;
        }

        // GET api/Cancelacion/detalle?tipo=Orden de compra&id=5
        // Devuelve el detalle para la ventana emergente de confirmación.
        [HttpGet("detalle")]
        public async Task<IActionResult> GetDetalle([FromQuery] string tipo, [FromQuery] int id)
        {
            var usuarioId = this.GetUsuarioId();
            if (usuarioId == null)
                return Unauthorized();

            var resultado = await _cancelacionService.GetDetalle(usuarioId.Value, tipo, id);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado.Data);
        }

        // POST api/Cancelacion -> confirma la cancelación.
        [HttpPost]
        public async Task<IActionResult> Cancelar([FromBody] CancelacionConfirmarDTO dto)
        {
            var usuarioId = this.GetUsuarioId();
            if (usuarioId == null)
                return Unauthorized();

            var resultado = await _cancelacionService.Cancelar(usuarioId.Value, dto);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            var can = resultado.Data!;
            await _notifService.EncolarAsync(
                usuarioId.Value,
                "Cancelacion",
                $"Cancelación de {can.TipoOperacion} completada",
                $"Tu {can.TipoOperacion} ({can.Par}) fue cancelada exitosamente. " +
                $"Cantidad cancelada: {can.CantidadCancelada}. " +
                $"Monto reembolsado: {can.MontoReembolsado} {can.MonedaReembolso}. " +
                $"Nuevo saldo: {can.NuevoSaldo} {can.MonedaReembolso}. " +
                $"Fecha: {can.FechaCancelacion:dd/MM/yyyy HH:mm}.",
                can.TipoOperacion == "Orden de compra" ? "OrdenCompra" : "OfertaVenta",
                can.CancelacionId);

            return Ok(can);
        }
    }
}
