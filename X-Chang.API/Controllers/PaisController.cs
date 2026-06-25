using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaisController : ControllerBase
    {
        private readonly ExchangeDivisasDbContext _context;

        public PaisController(ExchangeDivisasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var paises = await _context.Paises
                .Include(p => p.Moneda)
                .OrderBy(p => p.Nombre)
                .Select(p => new
                {
                    paisId = p.PaisId,
                    nombre = p.Nombre,
                    monedaId = p.MonedaId,
                    codigoMoneda = p.Moneda.CodigoIso
                })
                .ToListAsync();

            return Ok(paises);
        }
    }
}