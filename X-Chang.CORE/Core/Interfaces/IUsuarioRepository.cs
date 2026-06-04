using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuarios?> ObtenerPorIdAsync(int usuarioId);
        Task<Usuarios?> ObtenerConRolYPaisAsync(int usuarioId);
        Task ActualizarAsync(Usuarios usuario);
    }
}