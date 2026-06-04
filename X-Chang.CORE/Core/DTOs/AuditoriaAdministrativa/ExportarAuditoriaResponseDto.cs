using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.AuditoriaAdministrativa
{
    public class ExportarAuditoriaResponseDto
    {
        public string NombreArchivo { get; set; } = string.Empty;

        public string TipoContenido { get; set; } = string.Empty;

        public byte[] Archivo { get; set; } = Array.Empty<byte>();
    }
}