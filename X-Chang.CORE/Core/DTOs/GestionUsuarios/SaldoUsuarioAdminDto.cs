using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.GestionUsuarios
{
    public class SaldoUsuarioAdminDto
    {
        public int MonedaId { get; set; }

        public string CodigoMoneda { get; set; } = string.Empty;

        public string NombreMoneda { get; set; } = string.Empty;

        public decimal SaldoDisponible { get; set; }
    }
}