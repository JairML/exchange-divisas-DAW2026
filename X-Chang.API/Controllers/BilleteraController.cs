using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using X_Chang.API.Helpers;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    // US-006: Billetera virtual y barra de monedas.
    // [Authorize] se habilitará cuando esté integrado el login (US-001/US-002).
    [Route("api/[controller]")]
    [ApiController]
    public class BilleteraController : ControllerBase
    {
        private readonly IBilleteraService _billeteraService;

        public BilleteraController(IBilleteraService billeteraService)
        {
            _billeteraService = billeteraService;
        }

        // GET api/Billetera -> resumen de la billetera del usuario autenticado.
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var usuarioId = this.GetUsuarioId();
            if (usuarioId == null)
                return Unauthorized();

            var resumen = await _billeteraService.GetBilletera(usuarioId.Value);
            return Ok(resumen);
        }
    }
}
