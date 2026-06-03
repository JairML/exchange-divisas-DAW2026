using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.DTOs;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IConfiguracionUsuarioService
    {
        Task<TemaVisualResponseDto> ObtenerTemaVisualAsync(string tokenSesion);
        Task<TemaVisualResponseDto> ActualizarTemaVisualAsync(string tokenSesion, ActualizarTemaVisualRequestDto request);
    }
}