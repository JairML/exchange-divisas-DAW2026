namespace X_Chang.CORE.Core.DTOs.GestionUsuarios
{
    public class UsuarioAdminResumenDto
    {
        public int UsuarioId { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string CorreoElectronico { get; set; } = string.Empty;

        public string PaisResidencia { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public bool EsAdministrador { get; set; }

        public string TextoBotonAccion { get; set; } = string.Empty;
    }
}
