using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs
{
    public class TemaVisualResponseDto
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string TemaVisual { get; set; } = string.Empty;
    }
}