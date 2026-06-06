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
            await _notifService.EncolarAsync(
                UsuarioId,
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
