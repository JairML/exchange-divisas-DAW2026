using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BilleteraController : ControllerBase
    {
        private readonly IBilleteraService _billeteraService;

        public BilleteraController(IBilleteraService billeteraService)
        {
            _billeteraService = billeteraService;
        }

        private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var resumen = await _billeteraService.GetBilletera(UsuarioId);
            return Ok(resumen);
        }
    }
}
