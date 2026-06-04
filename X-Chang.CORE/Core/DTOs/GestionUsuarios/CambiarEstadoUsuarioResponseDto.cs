using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.GestionUsuarios
{
    public class CambiarEstadoUsuarioResponseDto
    {
        public int UsuarioId { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string EstadoAnterior { get; set; } = string.Empty;

        public string EstadoNuevo { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public DateTime FechaAccion { get; set; }
    }
}