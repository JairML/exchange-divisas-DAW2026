namespace X_Chang.CORE.Core.DTOs.Mercado
{
    public class PanelAdministrativoDto
    {
        public int TotalUsuariosRegistrados { get; set; }
        public int UsuariosActivosEnPeriodo { get; set; }
        public decimal TotalDepositos { get; set; }
        public decimal TotalRetiros { get; set; }
        public decimal VolumenTotalOperado { get; set; }
        public int OrdenesActivas { get; set; }
        public int OfertasActivas { get; set; }
        public int TransaccionesEjecutadas { get; set; }
        public List<VolumenPorMonedaDto> VolumenPorMoneda { get; set; } = new();
        public List<SerieOperacionesDiaDto> VolumenPorDia { get; set; } = new();
        public List<SerieOperacionesDiaDto> OperacionesPorDia { get; set; } = new();
    }

    public class VolumenPorMonedaDto
    {
        public int MonedaId { get; set; }
        public string CodigoMoneda { get; set; } = string.Empty;
        public decimal Volumen { get; set; }
    }

    public class SerieOperacionesDiaDto
    {
        public DateTime Fecha { get; set; }
        public decimal Volumen { get; set; }
        public int CantidadOperaciones { get; set; }
    }
}
