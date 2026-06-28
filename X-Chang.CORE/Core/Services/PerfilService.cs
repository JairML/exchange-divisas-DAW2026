using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Core.Services
{
    public class PerfilService : IPerfilService
    {
        private readonly ExchangeDivisasDbContext _context;

        public PerfilService(ExchangeDivisasDbContext context)
        {
            _context = context;
        }

        public async Task<PerfilDto> ObtenerPerfilAsync(int usuarioId)
        {
            var usuario = await ObtenerUsuarioAsync(usuarioId);
            return await MapearAsync(usuario);
        }

        public async Task<PerfilDto> ActualizarPerfilAsync(int usuarioId, ActualizarPerfilRequestDto request)
        {
            var usuario = await ObtenerUsuarioAsync(usuarioId);

            var nombre = request.NombreUsuario?.Trim() ?? string.Empty;
            if (nombre.Length < 2 || nombre.Length > 30)
                throw new ArgumentException("El nombre de usuario debe tener entre 2 y 30 caracteres.");

            usuario.NombreUsuario = nombre;
            usuario.Telefono = request.Telefono;
            usuario.FotoUrl = request.FotoUrl;

            await _context.SaveChangesAsync();
            return await MapearAsync(usuario);
        }

        private async Task<Usuarios> ObtenerUsuarioAsync(int usuarioId) =>
            await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        private async Task<PerfilDto> MapearAsync(Usuarios usuario)
        {
            var totalCompletadas = await _context.HistorialTransacciones
                .CountAsync(h => h.UsuarioId == usuario.UsuarioId && h.Estado == "Completada");

            return new PerfilDto
            {
                NombreUsuario = usuario.NombreUsuario,
                CorreoElectronico = usuario.CorreoElectronico,
                Telefono = usuario.Telefono,
                TipoDocumento = usuario.TipoDocumento,
                NumeroDocumento = usuario.NumeroDocumento,
                FotoUrl = usuario.FotoUrl,
                TotalTransaccionesCompletadas = totalCompletadas
            };
        }
    }
}
