using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public AuthRepository(ExchangeDivisasDbContext context) => _context = context;

    public Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario) =>
        _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);

    public Task<bool> ExisteCorreoAsync(string correo) =>
        _context.Usuarios.AnyAsync(u => u.CorreoElectronico == correo);

    public Task<Usuarios?> ObtenerPorCredencialAsync(string credencial) =>
        _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Pais)
            .FirstOrDefaultAsync(u =>
                u.NombreUsuario == credencial ||
                u.CorreoElectronico == credencial);

    public async Task<Usuarios> CrearUsuarioConBilleteraAsync(
        string nombreUsuario, string correo, string passwordHash, int paisId)
    {
        using var tx = await _context.Database.BeginTransactionAsync();

        var usuario = new Usuarios
        {
            NombreUsuario = nombreUsuario,
            CorreoElectronico = correo,
            PasswordHash = passwordHash,
            PaisId = paisId,
            RolId = 2,
            TemaVisual = "Claro",
            Estado = "Activo",
            FechaRegistro = DateTime.Now
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        _context.Billeteras.Add(new Billeteras
        {
            UsuarioId = usuario.UsuarioId,
            FechaCreacion = DateTime.Now
        });

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        return await _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Pais)
            .FirstAsync(u => u.UsuarioId == usuario.UsuarioId);
    }

    public async Task RegistrarAccesoAsync(
        int usuarioId, bool exitoso, string metodoIngreso, string? mensaje = null)
    {
        _context.AccesosUsuario.Add(new AccesosUsuario
        {
            UsuarioId = usuarioId,
            FechaAcceso = DateTime.Now,
            Exitoso = exitoso,
            MetodoIngreso = metodoIngreso,
            MensajeResultado = mensaje
        });

        await _context.SaveChangesAsync();
    }

    public async Task ActualizarUltimoAccesoAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario != null)
        {
            usuario.FechaUltimoAcceso = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}
