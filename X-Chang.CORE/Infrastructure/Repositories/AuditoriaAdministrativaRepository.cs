using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.AuditoriaAdministrativa;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class AuditoriaAdministrativaRepository : IAuditoriaAdministrativaRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public AuditoriaAdministrativaRepository(ExchangeDivisasDbContext context)
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

    public async Task<AuditoriaAdminPaginadoDto> BuscarAuditoriaAsync(FiltroAuditoriaAdminDto filtro)
    {
        var query = _context.AuditoriaAdministrativa
            .Include(a => a.Administrador)
            .Include(a => a.UsuarioAfectado)
            .AsQueryable();

        if (filtro.FechaDesde.HasValue)
            query = query.Where(a => a.FechaHora >= filtro.FechaDesde.Value);

        if (filtro.FechaHasta.HasValue)
            query = query.Where(a => a.FechaHora <= filtro.FechaHasta.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Administrador))
            query = query.Where(a => a.Administrador.NombreUsuario.Contains(filtro.Administrador));

        if (!string.IsNullOrWhiteSpace(filtro.UsuarioAfectado))
            query = query.Where(a => a.UsuarioAfectado.NombreUsuario.Contains(filtro.UsuarioAfectado));

        var tipoAccion = filtro.TipoAccion?.Trim() ?? "Todos";
        if (tipoAccion != "Todos")
            query = query.Where(a => a.TipoAccion == tipoAccion);

        query = query.OrderByDescending(a => a.FechaHora);

        var totalRegistros = await query.CountAsync();

        List<AuditoriaAdminRegistroDto> registros;

        if (filtro.RegistrosPorPagina == "Todos")
        {
            registros = await query.Select(a => new AuditoriaAdminRegistroDto
            {
                AuditoriaId = a.AuditoriaId,
                FechaHora = a.FechaHora,
                AdministradorId = a.AdministradorId,
                Administrador = a.Administrador.NombreUsuario,
                UsuarioAfectadoId = a.UsuarioAfectadoId,
                UsuarioAfectado = a.UsuarioAfectado.NombreUsuario,
                TipoAccion = a.TipoAccion,
                MensajeRegistrado = a.MensajeRegistrado
            }).ToListAsync();

            return new AuditoriaAdminPaginadoDto
            {
                Registros = registros,
                PaginaActual = 1,
                TotalPaginas = 1,
                TotalRegistros = totalRegistros,
                RegistrosPorPagina = filtro.RegistrosPorPagina,
                TienePaginaAnterior = false,
                TienePaginaSiguiente = false,
                Mensaje = $"Se encontraron {totalRegistros} registros."
            };
        }

        var porPagina = int.Parse(filtro.RegistrosPorPagina);
        var totalPaginas = totalRegistros == 0 ? 1 : (int)Math.Ceiling((double)totalRegistros / porPagina);
        var pagina = Math.Max(1, Math.Min(filtro.Pagina, totalPaginas));

        registros = await query
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(a => new AuditoriaAdminRegistroDto
            {
                AuditoriaId = a.AuditoriaId,
                FechaHora = a.FechaHora,
                AdministradorId = a.AdministradorId,
                Administrador = a.Administrador.NombreUsuario,
                UsuarioAfectadoId = a.UsuarioAfectadoId,
                UsuarioAfectado = a.UsuarioAfectado.NombreUsuario,
                TipoAccion = a.TipoAccion,
                MensajeRegistrado = a.MensajeRegistrado
            }).ToListAsync();

        return new AuditoriaAdminPaginadoDto
        {
            Registros = registros,
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = totalRegistros,
            RegistrosPorPagina = filtro.RegistrosPorPagina,
            TienePaginaAnterior = pagina > 1,
            TienePaginaSiguiente = pagina < totalPaginas,
            Mensaje = $"Se encontraron {totalRegistros} registros."
        };
    }

    public async Task<List<AuditoriaAdminRegistroDto>> BuscarAuditoriaParaExportarAsync(ExportarAuditoriaRequestDto filtro)
    {
        var query = _context.AuditoriaAdministrativa
            .Include(a => a.Administrador)
            .Include(a => a.UsuarioAfectado)
            .AsQueryable();

        if (filtro.FechaDesde.HasValue)
            query = query.Where(a => a.FechaHora >= filtro.FechaDesde.Value);

        if (filtro.FechaHasta.HasValue)
            query = query.Where(a => a.FechaHora <= filtro.FechaHasta.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Administrador))
            query = query.Where(a => a.Administrador.NombreUsuario.Contains(filtro.Administrador));

        if (!string.IsNullOrWhiteSpace(filtro.UsuarioAfectado))
            query = query.Where(a => a.UsuarioAfectado.NombreUsuario.Contains(filtro.UsuarioAfectado));

        var tipoAccion = filtro.TipoAccion?.Trim() ?? "Todos";
        if (tipoAccion != "Todos")
            query = query.Where(a => a.TipoAccion == tipoAccion);

        return await query
            .OrderByDescending(a => a.FechaHora)
            .Select(a => new AuditoriaAdminRegistroDto
            {
                AuditoriaId = a.AuditoriaId,
                FechaHora = a.FechaHora,
                AdministradorId = a.AdministradorId,
                Administrador = a.Administrador.NombreUsuario,
                UsuarioAfectadoId = a.UsuarioAfectadoId,
                UsuarioAfectado = a.UsuarioAfectado.NombreUsuario,
                TipoAccion = a.TipoAccion,
                MensajeRegistrado = a.MensajeRegistrado
            }).ToListAsync();
    }
}
