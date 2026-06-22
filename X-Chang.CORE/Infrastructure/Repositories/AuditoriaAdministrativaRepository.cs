using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.AuditoriaAdministrativa;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories
{
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
                .AnyAsync(u =>
                    u.UsuarioId == usuarioId &&
                    u.Estado == "Activo" &&
                    u.Rol.Nombre == "Administrador");
        }

        public async Task<AuditoriaAdminPaginadoDto> BuscarAuditoriaAsync(
            FiltroAuditoriaAdminDto filtro)
        {
            var query = ConstruirQueryBase(
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.Administrador,
                filtro.UsuarioAfectado,
                filtro.TipoAccion);

            var totalRegistros = await query.CountAsync();

            var registrosPorPagina = ObtenerRegistrosPorPagina(filtro.RegistrosPorPagina, totalRegistros);
            var totalPaginas = registrosPorPagina == 0
                ? 1
                : (int)Math.Ceiling(totalRegistros / (decimal)registrosPorPagina);

            if (filtro.Pagina > totalPaginas)
                filtro.Pagina = totalPaginas;

            if (filtro.Pagina < 1)
                filtro.Pagina = 1;

            var registrosQuery = query
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
                });

            List<AuditoriaAdminRegistroDto> registros;

            if (filtro.RegistrosPorPagina == "Todos")
            {
                registros = await registrosQuery.ToListAsync();
            }
            else
            {
                registros = await registrosQuery
                    .Skip((filtro.Pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .ToListAsync();
            }

            return new AuditoriaAdminPaginadoDto
            {
                Registros = registros,
                PaginaActual = filtro.Pagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros,
                RegistrosPorPagina = filtro.RegistrosPorPagina,
                TienePaginaAnterior = filtro.Pagina > 1,
                TienePaginaSiguiente = filtro.Pagina < totalPaginas,
                Mensaje = registros.Any()
                    ? string.Empty
                    : "No se encontraron registros de auditoría"
            };
        }

        public async Task<List<AuditoriaAdminRegistroDto>> BuscarAuditoriaParaExportarAsync(
            ExportarAuditoriaRequestDto filtro)
        {
            return await ConstruirQueryBase(
                    filtro.FechaDesde,
                    filtro.FechaHasta,
                    filtro.Administrador,
                    filtro.UsuarioAfectado,
                    filtro.TipoAccion)
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
                })
                .ToListAsync();
        }

        private IQueryable<Core.Entities.AuditoriaAdministrativa> ConstruirQueryBase(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            string? administrador,
            string? usuarioAfectado,
            string? tipoAccion)
        {
            var query = _context.AuditoriaAdministrativa
                .Include(a => a.Administrador)
                .Include(a => a.UsuarioAfectado)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(a => a.FechaHora >= fechaDesde.Value.Date);

            if (fechaHasta.HasValue)
                query = query.Where(a => a.FechaHora < fechaHasta.Value.Date.AddDays(1));

            if (!string.IsNullOrWhiteSpace(administrador))
            {
                var admin = administrador.Trim();
                query = query.Where(a => a.Administrador.NombreUsuario.Contains(admin));
            }

            if (!string.IsNullOrWhiteSpace(usuarioAfectado))
            {
                var afectado = usuarioAfectado.Trim();
                query = query.Where(a => a.UsuarioAfectado.NombreUsuario.Contains(afectado));
            }

            if (!string.IsNullOrWhiteSpace(tipoAccion) && tipoAccion != "Todos")
                query = query.Where(a => a.TipoAccion == tipoAccion);

            return query;
        }

        private static int ObtenerRegistrosPorPagina(string valor, int totalRegistros)
        {
            if (valor == "Todos")
                return totalRegistros == 0 ? 1 : totalRegistros;

            return int.Parse(valor);
        }
    }
}