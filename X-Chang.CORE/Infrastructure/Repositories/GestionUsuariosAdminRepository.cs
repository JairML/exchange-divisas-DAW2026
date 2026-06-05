using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.GestionUsuarios;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class GestionUsuariosAdminRepository : IGestionUsuariosAdminRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public GestionUsuariosAdminRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<bool> EsAdministradorActivoAsync(int usuarioId)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.UsuarioId == usuarioId
                        && u.Estado == "Activo"
                        && u.Rol.Nombre == "Administrador");
    }

    public async Task<List<UsuarioAdminResumenDto>> BuscarUsuariosAsync(FiltroUsuariosAdminDto filtro)
    {
        var query = _context.Usuarios
            .Include(u => u.Pais)
            .Where(u => u.Rol.Nombre != "Administrador")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.NombreUsuario))
            query = query.Where(u => u.NombreUsuario.Contains(filtro.NombreUsuario));

        if (!string.IsNullOrWhiteSpace(filtro.CorreoElectronico))
            query = query.Where(u => u.CorreoElectronico.Contains(filtro.CorreoElectronico));

        var estado = filtro.Estado?.Trim() ?? "Todos";
        if (estado != "Todos")
            query = query.Where(u => u.Estado == estado);

        return await query
            .OrderBy(u => u.NombreUsuario)
            .Select(u => new UsuarioAdminResumenDto
            {
                UsuarioId = u.UsuarioId,
                NombreUsuario = u.NombreUsuario,
                CorreoElectronico = u.CorreoElectronico,
                PaisResidencia = u.Pais.Nombre,
                Estado = u.Estado,
                TextoBotonAccion = u.Estado == "Activo" ? "Restringir" : "Habilitar"
            }).ToListAsync();
    }

    public async Task<UsuarioAdminDetalleDto?> ObtenerDetalleUsuarioAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Pais)
            .Include(u => u.Billeteras)
                .ThenInclude(b => b!.SaldosBilletera)
                    .ThenInclude(s => s.Moneda)
            .Include(u => u.HistorialTransacciones)
                .ThenInclude(h => h.ParMoneda)
                    .ThenInclude(p => p!.MonedaOrigen)
            .Include(u => u.HistorialTransacciones)
                .ThenInclude(h => h.ParMoneda)
                    .ThenInclude(p => p!.MonedaDestino)
            .Include(u => u.HistorialTransacciones)
                .ThenInclude(h => h.Moneda)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

        if (usuario == null)
            return null;

        var saldos = usuario.Billeteras?.SaldosBilletera
            .Select(s => new SaldoUsuarioAdminDto
            {
                MonedaId = s.MonedaId,
                CodigoMoneda = s.Moneda.CodigoIso,
                NombreMoneda = s.Moneda.Nombre,
                SaldoDisponible = s.SaldoDisponible
            }).ToList() ?? new List<SaldoUsuarioAdminDto>();

        var historial = usuario.HistorialTransacciones
            .OrderByDescending(h => h.FechaHora)
            .Take(50)
            .Select(h => new HistorialUsuarioAdminDto
            {
                HistorialId = h.HistorialId,
                TipoOperacion = h.TipoOperacion,
                ReferenciaId = h.ReferenciaId,
                ParMonedaId = h.ParMonedaId,
                ParMoneda = h.ParMoneda != null
                    ? $"{h.ParMoneda.MonedaOrigen.CodigoIso}/{h.ParMoneda.MonedaDestino.CodigoIso}"
                    : null,
                MonedaId = h.MonedaId,
                Moneda = h.Moneda?.CodigoIso,
                Estado = h.Estado,
                MetodoEjecucion = h.MetodoEjecucion,
                FechaHora = h.FechaHora
            }).ToList();

        return new UsuarioAdminDetalleDto
        {
            UsuarioId = usuario.UsuarioId,
            NombreUsuario = usuario.NombreUsuario,
            CorreoElectronico = usuario.CorreoElectronico,
            PaisResidencia = usuario.Pais.Nombre,
            Estado = usuario.Estado,
            Saldos = saldos,
            HistorialTransacciones = historial
        };
    }

    public async Task<CambiarEstadoUsuarioResponseDto> RestringirUsuarioAsync(
        int administradorId,
        int usuarioId,
        string mensaje)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado.");

            var estadoAnterior = usuario.Estado;
            usuario.Estado = "Restringido";

            var restriccion = new RestriccionesUsuario
            {
                UsuarioId = usuarioId,
                AdministradorId = administradorId,
                TipoAccion = "Restriccion",
                Mensaje = mensaje,
                FechaInicio = DateTime.UtcNow,
                EstadoRestriccion = "Activa"
            };
            _context.RestriccionesUsuario.Add(restriccion);

            var auditoria = new AuditoriaAdministrativa
            {
                AdministradorId = administradorId,
                UsuarioAfectadoId = usuarioId,
                TipoAccion = "Restricción",
                MensajeRegistrado = mensaje,
                FechaHora = DateTime.UtcNow
            };
            _context.AuditoriaAdministrativa.Add(auditoria);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new CambiarEstadoUsuarioResponseDto
            {
                UsuarioId = usuario.UsuarioId,
                NombreUsuario = usuario.NombreUsuario,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = "Restringido",
                Mensaje = mensaje,
                FechaAccion = DateTime.UtcNow
            };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<CambiarEstadoUsuarioResponseDto> HabilitarUsuarioAsync(
        int administradorId,
        int usuarioId,
        string mensaje)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado.");

            var estadoAnterior = usuario.Estado;
            usuario.Estado = "Activo";

            // Cerrar restricciones activas
            var restriccionesActivas = await _context.RestriccionesUsuario
                .Where(r => r.UsuarioId == usuarioId && r.EstadoRestriccion == "Activa")
                .ToListAsync();

            foreach (var restriccion in restriccionesActivas)
            {
                restriccion.EstadoRestriccion = "Levantada";
                restriccion.FechaFin = DateTime.UtcNow;
            }

            var auditoria = new AuditoriaAdministrativa
            {
                AdministradorId = administradorId,
                UsuarioAfectadoId = usuarioId,
                TipoAccion = "Habilitación",
                MensajeRegistrado = mensaje,
                FechaHora = DateTime.UtcNow
            };
            _context.AuditoriaAdministrativa.Add(auditoria);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new CambiarEstadoUsuarioResponseDto
            {
                UsuarioId = usuario.UsuarioId,
                NombreUsuario = usuario.NombreUsuario,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = "Activo",
                Mensaje = mensaje,
                FechaAccion = DateTime.UtcNow
            };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
