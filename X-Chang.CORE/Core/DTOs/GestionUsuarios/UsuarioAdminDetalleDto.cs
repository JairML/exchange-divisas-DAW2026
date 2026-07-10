namespace X_Chang.CORE.Core.DTOs.GestionUsuarios
{
    public class UsuarioAdminDetalleDto
    {
        public int UsuarioId { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string CorreoElectronico { get; set; } = string.Empty;

        public string PaisResidencia { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public bool EsAdministrador { get; set; }

        public List<SaldoUsuarioAdminDto> Saldos { get; set; } = new();

        public List<HistorialUsuarioAdminDto> HistorialTransacciones { get; set; } = new();

        public List<HistorialRestriccionAdminDto> HistorialRestricciones { get; set; } = new();
    }
}
