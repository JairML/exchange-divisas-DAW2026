using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories
{
    public class SesionUsuarioRepository : ISesionUsuarioRepository
    {
        private readonly ExchangeDivisasDbContext _context;

        public SesionUsuarioRepository(
            ExchangeDivisasDbContext context)
        {
            _context = context;
        }

        public async Task<SesionesUsuario?> ObtenerSesionActivaAsync(
            string tokenSesion)
        {
            return await _context.SesionesUsuario
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s =>
                    s.TokenSesion == tokenSesion &&
                    s.Estado == "Activa" &&
                    s.FechaExpiracion > DateTime.Now);
        }

        public async Task<SesionesUsuario> CrearSesionAsync(
            int usuarioId,
            string tokenSesion,
            DateTime fechaExpiracion)
        {
            var sesion = new SesionesUsuario
            {
                UsuarioId = usuarioId,
                TokenSesion = tokenSesion,
                FechaInicio = DateTime.Now,
                FechaExpiracion = fechaExpiracion,
                Estado = "Activa"
            };

            _context.SesionesUsuario.Add(sesion);

            await _context.SaveChangesAsync();

            return sesion;
        }

        public async Task<bool> CerrarSesionAsync(
            string tokenSesion)
        {
            var sesion = await _context.SesionesUsuario
                .FirstOrDefaultAsync(s =>
                    s.TokenSesion == tokenSesion &&
                    s.Estado == "Activa");

            if (sesion == null)
                return false;

            sesion.Estado = "Cerrada";
            sesion.FechaCierre = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExisteSesionActivaAsync(
            string tokenSesion)
        {
            return await _context.SesionesUsuario
                .AnyAsync(s =>
                    s.TokenSesion == tokenSesion &&
                    s.Estado == "Activa" &&
                    s.FechaExpiracion > DateTime.Now);
        }
    }
}