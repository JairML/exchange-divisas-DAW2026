using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    // Lista de monedas activas para poblar los desplegables del front.
    // NOTA DE EQUIPO: este endpoint es transversal; si otro integrante también crea un
    // controlador de monedas, conviene unificar en el PR para evitar rutas duplicadas.
    [Route("api/[controller]")]
    [ApiController]
    public class MonedaController : ControllerBase
    {
        private readonly IMonedaService _monedaService;

        public MonedaController(IMonedaService monedaService)
        {
            _monedaService = monedaService;
        }

        // GET api/Moneda
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var monedas = await _monedaService.GetMonedas();
            return Ok(monedas);
        }
    }
}
