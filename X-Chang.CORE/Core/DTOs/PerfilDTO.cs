namespace X_Chang.CORE.Core.DTOs
{
    public class PerfilDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? TipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? FotoUrl { get; set; }
        public int TotalTransaccionesCompletadas { get; set; }
    }

    public class ActualizarPerfilRequestDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? FotoUrl { get; set; }
    }
}
