using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories
{
    public class SesionUsuarioRepository : ISesionUsuarioRepository
    {
        private readonly ExchangeDivisasDbContext _context;

        public SesionUsuarioRepository(ExchangeDivisasDbContext context)
        {
            _context = context;
        }

        public async Task<SesionesUsuario?> ObtenerSesionActivaAsync(string tokenSesion)
        {
            return await _context.SesionesUsuario
                .FirstOrDefaultAsync(s =>
                    s.TokenSesion == tokenSesion &&
                    s.Estado == "Activa" &&
                    s.FechaCierre == null &&
                    s.FechaExpiracion > DateTime.Now);
        }
    }
}