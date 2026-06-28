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
        public List<DistribucionTipoOperacionDto> DistribucionPorTipo { get; set; } = new();
        public List<MonedaResumenAdminDto> Monedas { get; set; } = new();
        public List<MejorRutaAdminDto> MejoresRutas { get; set; } = new();
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

    public class DistribucionTipoOperacionDto
    {
        public string TipoOperacion { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class MonedaResumenAdminDto
    {
        public int MonedaId { get; set; }
        public string CodigoMoneda { get; set; } = string.Empty;
        public decimal VolumenOperado { get; set; }
        public int CantidadOperaciones { get; set; }
        public decimal CantidadComprada { get; set; }
        public decimal CantidadVendida { get; set; }
        public decimal TotalDepositado { get; set; }
        public decimal TotalRetirado { get; set; }
    }

    public class MejorRutaAdminDto
    {
        public DateTime FechaCreacion { get; set; }
        public string MonedaInicial { get; set; } = string.Empty;
        public string MonedaFinal { get; set; } = string.Empty;
        public int CantidadSaltos { get; set; }
        public decimal? AhorroEstimado { get; set; }
        public decimal? GananciaEstimada { get; set; }
        public List<SaltoMejorRutaAdminDto> Saltos { get; set; } = new();
    }

    public class SaltoMejorRutaAdminDto
    {
        public int NumeroSalto { get; set; }
        public string Par { get; set; } = string.Empty;
        public decimal CantidadConvertida { get; set; }
        public decimal? PrecioMinimo { get; set; }
        public decimal? PrecioMaximo { get; set; }
        public decimal? PrecioPromedio { get; set; }
        public decimal ResultadoObtenido { get; set; }
    }

    public class ActividadRecienteAdminDto
    {
        public DateTime FechaHora { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string TipoOperacion { get; set; } = string.Empty;
        public string? Par { get; set; }
        public decimal MontoTotal { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class FiltroActividadRecienteDto
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int Pagina { get; set; } = 1;
        public int RegistrosPorPagina { get; set; } = 20;
    }

    public class ActividadRecientePaginadaDto
    {
        public List<ActividadRecienteAdminDto> Registros { get; set; } = new();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    public class ExportarPanelAdminRequestDto
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
    }

    public class ExportarPanelAdminResponseDto
    {
        public string NombreArchivo { get; set; } = string.Empty;
        public string TipoContenido { get; set; } = string.Empty;
        public byte[] Archivo { get; set; } = Array.Empty<byte>();
    }
}
