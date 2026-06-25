namespace X_Chang.CORE.Core.DTOs.CompraInmediata
{
    public class TiempoEstimadoBusquedaRutaDto
    {
        public int CantidadMaximaSaltos { get; set; }

        public int CantidadMonedas { get; set; }

        public long RutasEstimadas { get; set; }

        public int TiempoEstimadoMs { get; set; }

        public decimal TiempoEstimadoSegundos { get; set; }

        public string TiempoEstimadoTexto { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;
    }
}