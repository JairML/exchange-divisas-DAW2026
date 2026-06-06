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
            await _notifService.EncolarAsync(
                UsuarioId,
                "Retiro",
                $"Retiro de {ret.CodigoISO} completado",
                $"Tu retiro de {ret.MontoRetirado} {ret.CodigoISO} fue registrado exitosamente. " +
                $"Comisión aplicada: {ret.ComisionAplicada}. Monto final a recibir: {ret.MontoFinalRecibido} {ret.CodigoISO}. " +
                $"Nuevo saldo: {ret.NuevoSaldo} {ret.CodigoISO}. Fecha: {ret.FechaRetiro:dd/MM/yyyy HH:mm}.",
                "Retiro",
                ret.RetiroId);

            return Ok(ret);
        }
    }
}
