namespace X_Chang.CORE.Core.DTOs.GestionUsuarios
{
    public class HistorialRestriccionAdminDto
    {
        public int RestriccionId { get; set; }

        public string TipoAccion { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public string EstadoRestriccion { get; set; } = string.Empty;

        public string Administrador { get; set; } = string.Empty;
    }
}
