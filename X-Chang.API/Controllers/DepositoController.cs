using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using X_Chang.API.Helpers;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    // US-007: Depósito de dinero a la billetera.
    // [Authorize] se habilitará cuando esté integrado el login (US-001/US-002).
    [Route("api/[controller]")]
    [ApiController]
    public class DepositoController : ControllerBase
    {
        private readonly IDepositoService _depositoService;
        private readonly INotificacionesCorreoService _notifService;

        public DepositoController(
            IDepositoService depositoService,
            INotificacionesCorreoService notifService)
        {
            _depositoService = depositoService;
            _notifService = notifService;
        }

        // GET api/Deposito/metodos-pago -> métodos de pago habilitados para el país del usuario.
        [HttpGet("metodos-pago")]
        public async Task<IActionResult> GetMetodosPago()
        {
            var usuarioId = this.GetUsuarioId();
            if (usuarioId == null)
                return Unauthorized();

            var metodos = await _depositoService.GetMetodosPago(usuarioId.Value);
            return Ok(metodos);
        }

        // POST api/Deposito/calcular -> comisión y total a pagar (resumen previo).
        [HttpPost("calcular")]
        public async Task<IActionResult> Calcular([FromBody] DepositoCalcularDTO dto)
        {
            var usuarioId = this.GetUsuarioId();
            if (usuarioId == null)
                return Unauthorized();

            var resultado = await _depositoService.Calcular(usuarioId.Value, dto);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado.Data);
        }

        // POST api/Deposito -> confirma y registra el depósito.
        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] DepositoCreateDTO dto)
        {
            var usuarioId = this.GetUsuarioId();
            if (usuarioId == null)
                return Unauthorized();

            var resultado = await _depositoService.RegistrarDeposito(usuarioId.Value, dto);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            var dep = resultado.Data!;
            await _notifService.EncolarAsync(
                usuarioId.Value,
                "Deposito",
                $"Depósito de {dep.CodigoISO} completado",
                $"Tu depósito de {dep.MontoDepositado} {dep.CodigoISO} fue registrado exitosamente. " +
                $"Comisión aplicada: {dep.ComisionAplicada}. Total pagado: {dep.TotalPagado}. " +
                $"Nuevo saldo: {dep.NuevoSaldo} {dep.CodigoISO}. Fecha: {dep.FechaDeposito:dd/MM/yyyy HH:mm}.",
                "Deposito",
                dep.DepositoId);

            return Ok(dep);
        }
    }
}
