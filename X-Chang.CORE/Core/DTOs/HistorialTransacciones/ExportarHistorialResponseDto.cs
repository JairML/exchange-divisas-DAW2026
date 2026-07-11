namespace X_Chang.CORE.Core.DTOs.HistorialTransacciones
{
    public class ExportarHistorialResponseDto
    {
        public string NombreArchivo { get; set; } = string.Empty;
        public string TipoContenido { get; set; } = string.Empty;
        public byte[] Archivo { get; set; } = Array.Empty<byte>();
    }
}
