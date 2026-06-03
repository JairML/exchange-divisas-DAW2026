using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface ISesionUsuarioRepository
    {
        Task<SesionesUsuario?> ObtenerSesionActivaAsync(string tokenSesion);
    }
}