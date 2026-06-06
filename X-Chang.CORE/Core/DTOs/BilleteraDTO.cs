using System.Collections.Generic;

namespace X_Chang.CORE.Core.DTOs
{
    public class SaldoMonedaDTO
    {
        public int MonedaId { get; set; }
        public string CodigoISO { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal SaldoDisponible { get; set; }
    }

    public class BilleteraResumenDTO
    {
        public int UsuarioId { get; set; }
        public int BilleteraId { get; set; }
        public bool TieneFondos { get; set; }
        public List<SaldoMonedaDTO> Saldos { get; set; } = new();
        public List<SaldoMonedaDTO> SaldosConFondos { get; set; } = new();
    }
}
