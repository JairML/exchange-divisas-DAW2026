using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using X_Chang.API.Helpers;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    // US-008: Retiro de dinero de la billetera.
    // [Authorize] se habilitará cuando esté integrado el login (US-001/US-002).
    [Route("api/[controller]")]
    [ApiController]
    public class RetiroController : ControllerBase
    {
        private readonly IRetiroService _retiroService;
        private readonly INotificacionesCorreoService _notifService;

        public RetiroController(
            IRetiroService retiroService,
            INotificacionesCorreoService notifService)
        {
            _retiroService = retiroService;
            _notifService = notifService;
        }

        // GET api/Retiro/metodos-cobro -> métodos de cobro habilitados para el país del usuario.
        [HttpGet("metodos-cobro")]
        public async Task<IActionResult> GetMetodosCobro()
        {
            var usuarioId = this.GetUsuarioId();
            if (usuarioId == null)
                return Unauthorized();

            var metodos = await _retiroService.GetMetodosCobro(usuarioId.Value);
            return Ok(metodos);
        }

        // POST api/Retiro/calcular -> comisión y monto final a recibir (resumen previo).
        [HttpPost("calcular")]
        public async Task<IActionResult> Calcular([FromBody] RetiroCalcularDTO dto)
        {
            var usuarioId = this.GetUsuarioId();
            if (usuarioId == null)
                return Unauthorized();

            var resultado = await _retiroService.Calcular(usuarioId.Value, dto);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado.Data);
        }

        // POST api/Retiro -> confirma y registra el retiro.
        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] RetiroCreateDTO dto)
        {
            var usuarioId = this.GetUsuarioId();
            if (usuarioId == null)
                return Unauthorized();

            var resultado = await _retiroService.RegistrarRetiro(usuarioId.Value, dto);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            var ret = resultado.Data!;
            await _notifService.EncolarAsync(
                usuarioId.Value,
                "Retiro",
                $"Retiro de {ret.CodigoISO} completado",
                $"Tu retiro de {ret.MontoRetirado} {ret.CodigoISO} fue procesado exitosamente. " +
                $"Comisión aplicada: {ret.ComisionAplicada}. Monto final recibido: {ret.MontoFinalRecibido} {ret.CodigoISO}. " +
                $"Nuevo saldo: {ret.NuevoSaldo} {ret.CodigoISO}. Fecha: {ret.FechaRetiro:dd/MM/yyyy HH:mm}.",
                "Retiro",
                ret.RetiroId);

            return Ok(ret);
        }
    }
}
