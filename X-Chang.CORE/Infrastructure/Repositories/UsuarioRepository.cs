using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ExchangeDivisasDbContext _context;

        public UsuarioRepository(ExchangeDivisasDbContext context)
        {
            _context = context;
        }

        public async Task<Usuarios?> ObtenerPorIdAsync(int usuarioId)
        {
            return await _context.Usuarios
                .Include(u => u.Pais)
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
        }

        public async Task ActualizarAsync(Usuarios usuario)
        {
            // Si la entidad fue cargada en este mismo scope (contexto scoped por request),
            // ya está tracked: Update() es redundante pero inofensivo.
            // Si llega detached (otro scope), Update() la adjunta y marca todo como Modified.
            if (_context.Entry(usuario).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                _context.Usuarios.Update(usuario);

            await _context.SaveChangesAsync();
        }
    }
}