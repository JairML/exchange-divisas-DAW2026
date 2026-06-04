using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    // Servicio de monedas: alimenta las listas desplegables del front (US-006/US-007).
    public class MonedaService : IMonedaService
    {
        private readonly IMonedaRepository _monedaRepository;

        public MonedaService(IMonedaRepository monedaRepository)
        {
            _monedaRepository = monedaRepository;
        }

        public async Task<IEnumerable<MonedaDTO>> GetMonedas()
        {
            var monedas = await _monedaRepository.GetMonedasActivas();

            return monedas.Select(m => new MonedaDTO
            {
                MonedaId = m.MonedaId,
                CodigoISO = m.CodigoIso,
                Nombre = m.Nombre,
                Tipo = m.Tipo
            }).ToList();
        }
    }
}
