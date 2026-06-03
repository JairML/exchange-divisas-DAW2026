using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace X_Chang.CORE.Core.DTOs
{
    public class ActualizarTemaVisualRequestDto
    {
        [Required]
        public string TemaVisual { get; set; } = string.Empty;
    }
}