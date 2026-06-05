using System.Linq;
using System.Threading.Tasks;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    // US-006: Billetera virtual y barra de monedas.
    public class BilleteraService : IBilleteraService
    {
        private readonly IBilleteraRepository _billeteraRepository;

        public BilleteraService(IBilleteraRepository billeteraRepository)
        {
            _billeteraRepository = billeteraRepository;
        }

        public async Task<BilleteraResumenDTO> GetBilletera(int usuarioId)
        {
            var billetera = await _billeteraRepository.GetBilleteraByUsuario(usuarioId);
            var saldos = await _billeteraRepository.GetSaldosByUsuario(usuarioId);

            // Lista completa de monedas, ordenada de mayor a menor saldo y luego por código.
            var todos = saldos
                .Select(s => new SaldoMonedaDTO
                {
                    MonedaId = s.MonedaId,
                    CodigoISO = s.Moneda.CodigoIso,
                    Nombre = s.Moneda.Nombre,
                    SaldoDisponible = s.SaldoDisponible
                })
                .OrderByDescending(s => s.SaldoDisponible)
                .ThenBy(s => s.CodigoISO)
                .ToList();

            // Solo las monedas con fondos (barra superior).
            var conFondos = todos.Where(s => s.SaldoDisponible > 0m).ToList();

            return new BilleteraResumenDTO
            {
                UsuarioId = usuarioId,
                BilleteraId = billetera?.BilleteraId ?? 0,
                TieneFondos = conFondos.Count > 0,
                Saldos = todos,
                SaldosConFondos = conFondos
            };
        }
    }
}
