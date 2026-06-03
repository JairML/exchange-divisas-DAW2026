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
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
        }

        public async Task ActualizarAsync(Usuarios usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }
}