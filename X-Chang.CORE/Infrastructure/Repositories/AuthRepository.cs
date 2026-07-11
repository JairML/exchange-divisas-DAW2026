using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public AuthRepository(ExchangeDivisasDbContext context) => _context = context;

    public Task<bool> ExisteCorreoAsync(string correo) =>
        _context.Usuarios.AnyAsync(u => u.CorreoElectronico == correo);

    public Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario) =>
        _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);

    public Task<Roles?> ObtenerRolPorNombreAsync(string nombre) =>
        _context.Roles.FirstOrDefaultAsync(r => r.Nombre == nombre);

    public Task<Paises?> ObtenerPaisAsync(int paisId) =>
        _context.Paises.FindAsync(paisId).AsTask();

    public async Task<Usuarios> CrearUsuarioAsync(Usuarios usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task CrearBilleteraAsync(Billeteras billetera)
    {
        _context.Billeteras.Add(billetera);
        await _context.SaveChangesAsync();
    }

    public async Task RegistrarAccesoAsync(AccesosUsuario acceso)
    {
        _context.AccesosUsuario.Add(acceso);
        await _context.SaveChangesAsync();
    }

    public Task<Usuarios?> BuscarPorIdentificadorAsync(string identificador) =>
        _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u =>
                u.CorreoElectronico == identificador ||
                u.NombreUsuario == identificador);

    public async Task ActualizarFechaAccesoAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario != null)
        {
            usuario.FechaUltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public Task<Usuarios?> BuscarPorCorreoAsync(string correo) =>
        _context.Usuarios.FirstOrDefaultAsync(u => u.CorreoElectronico == correo);

    public async Task GuardarTokenRecuperacionAsync(int usuarioId, string token, DateTime expira)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario != null)
        {
            usuario.TokenRecuperacion = token;
            usuario.TokenRecuperacionExpira = expira;
            await _context.SaveChangesAsync();
        }
    }

    public Task<Usuarios?> BuscarPorTokenRecuperacionValidoAsync(string token) =>
        _context.Usuarios.FirstOrDefaultAsync(u =>
            u.TokenRecuperacion == token &&
            u.TokenRecuperacionExpira != null &&
            u.TokenRecuperacionExpira > DateTime.UtcNow);

    public async Task ActualizarPasswordYLimpiarTokenAsync(int usuarioId, string nuevoPasswordHash)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario != null)
        {
            usuario.PasswordHash = nuevoPasswordHash;
            usuario.TokenRecuperacion = null;
            usuario.TokenRecuperacionExpira = null;
            await _context.SaveChangesAsync();
        }
    }
}
