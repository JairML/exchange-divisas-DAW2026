using System.Collections.Generic;

namespace X_Chang.CORE.Core.DTOs
{
    // Saldo de una moneda dentro de la billetera del usuario.
    public class SaldoMonedaDTO
    {
        public int MonedaId { get; set; }
        public string CodigoISO { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal SaldoDisponible { get; set; }
    }

    // US-006: Billetera virtual y barra de monedas.
    public class BilleteraResumenDTO
    {
        public int UsuarioId { get; set; }
        public int BilleteraId { get; set; }

        // true si al menos una moneda tiene saldo > 0.
        // El front muestra "No existen fondos disponibles" cuando es false.
        public bool TieneFondos { get; set; }

        // Las 27 monedas ordenadas de mayor a menor saldo (lista completa desplegable).
        public List<SaldoMonedaDTO> Saldos { get; set; } = new();

        // Solo monedas con saldo > 0, ordenadas de mayor a menor (barra superior).
        public List<SaldoMonedaDTO> SaldosConFondos { get; set; } = new();
    }
}
