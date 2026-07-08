using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.GestionUsuarios;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories
{
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
                .AnyAsync(u =>
                    u.UsuarioId == usuarioId &&
                    u.Estado == "Activo" &&
                    u.Rol.Nombre == "ADM");
        }

        public async Task<List<UsuarioAdminResumenDto>> BuscarUsuariosAsync(FiltroUsuariosAdminDto filtro)
        {
            var query = _context.Usuarios
                .Include(u => u.Pais)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.NombreUsuario))
            {
                var nombre = filtro.NombreUsuario.Trim();
                query = query.Where(u => u.NombreUsuario.Contains(nombre));
            }

            if (!string.IsNullOrWhiteSpace(filtro.CorreoElectronico))
            {
                var correo = filtro.CorreoElectronico.Trim();
                query = query.Where(u => u.CorreoElectronico.Contains(correo));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Estado) && filtro.Estado != "Todos")
                query = query.Where(u => u.Estado == filtro.Estado);

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
                })
                .ToListAsync();
        }

        public async Task<UsuarioAdminDetalleDto?> ObtenerDetalleUsuarioAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Pais)
                .Include(u => u.Billeteras)
                    .ThenInclude(b => b.SaldosBilletera)
                        .ThenInclude(s => s.Moneda)
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

            if (usuario == null)
                return null;

            var historial = await _context.HistorialTransacciones
                .Include(h => h.ParMoneda)
                    .ThenInclude(p => p.MonedaOrigen)
                .Include(h => h.ParMoneda)
                    .ThenInclude(p => p.MonedaDestino)
                .Include(h => h.Moneda)
                .Where(h => h.UsuarioId == usuarioId)
                .OrderByDescending(h => h.FechaHora)
                .Select(h => new HistorialUsuarioAdminDto
                {
                    HistorialId = h.HistorialId,
                    TipoOperacion = h.TipoOperacion,
                    ReferenciaId = h.ReferenciaId,
                    ParMonedaId = h.ParMonedaId,
                    ParMoneda = h.ParMoneda == null
                        ? null
                        : h.ParMoneda.MonedaOrigen.CodigoIso + " → " + h.ParMoneda.MonedaDestino.CodigoIso,
                    MonedaId = h.MonedaId,
                    Moneda = h.Moneda == null ? null : h.Moneda.CodigoIso,
                    Estado = h.Estado,
                    MetodoEjecucion = h.MetodoEjecucion,
                    FechaHora = h.FechaHora
                })
                .ToListAsync();

            return new UsuarioAdminDetalleDto
            {
                UsuarioId = usuario.UsuarioId,
                NombreUsuario = usuario.NombreUsuario,
                CorreoElectronico = usuario.CorreoElectronico,
                PaisResidencia = usuario.Pais.Nombre,
                Estado = usuario.Estado,
                Saldos = usuario.Billeteras?.SaldosBilletera
                    .OrderBy(s => s.Moneda.CodigoIso)
                    .Select(s => new SaldoUsuarioAdminDto
                    {
                        MonedaId = s.MonedaId,
                        CodigoMoneda = s.Moneda.CodigoIso,
                        NombreMoneda = s.Moneda.Nombre,
                        SaldoDisponible = s.SaldoDisponible
                    })
                    .ToList() ?? new List<SaldoUsuarioAdminDto>(),
                HistorialTransacciones = historial
            };
        }

        public async Task<CambiarEstadoUsuarioResponseDto> RestringirUsuarioAsync(
            int administradorId,
            int usuarioId,
            string mensaje)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

            if (usuario == null)
                throw new ArgumentException("El usuario no existe.");

            if (usuario.UsuarioId == administradorId)
                throw new InvalidOperationException("El administrador no puede restringirse a sí mismo.");

            if (usuario.Rol.Nombre == "ADM")
                throw new InvalidOperationException("No es posible restringir una cuenta de administrador.");

            if (usuario.Estado == "Restringido")
                throw new InvalidOperationException("El usuario ya está restringido.");

            var estadoAnterior = usuario.Estado;
            var fecha = DateTime.Now;

            usuario.Estado = "Restringido";

            await CancelarOrdenesActivasPorRestriccionAsync(usuarioId, fecha);
            await CancelarOfertasActivasPorRestriccionAsync(usuarioId, fecha);

            _context.RestriccionesUsuario.Add(new RestriccionesUsuario
            {
                UsuarioId = usuarioId,
                AdministradorId = administradorId,
                TipoAccion = "Restricción",
                Mensaje = mensaje,
                FechaInicio = fecha,
                EstadoRestriccion = "Activa"
            });

            _context.AuditoriaAdministrativa.Add(new AuditoriaAdministrativa
            {
                AdministradorId = administradorId,
                UsuarioAfectadoId = usuarioId,
                TipoAccion = "Restricción",
                MensajeRegistrado = mensaje,
                FechaHora = fecha
            });

            await CrearNotificacionAsync(
                usuarioId,
                "Restricción",
                "Cuenta restringida",
                mensaje,
                "RestriccionesUsuario",
                usuarioId);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new CambiarEstadoUsuarioResponseDto
            {
                UsuarioId = usuario.UsuarioId,
                NombreUsuario = usuario.NombreUsuario,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = usuario.Estado,
                Mensaje = mensaje,
                FechaAccion = fecha
            };
        }

        public async Task<CambiarEstadoUsuarioResponseDto> HabilitarUsuarioAsync(
            int administradorId,
            int usuarioId,
            string mensaje)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

            if (usuario == null)
                throw new ArgumentException("El usuario no existe.");

            if (usuario.Estado == "Activo")
                throw new InvalidOperationException("El usuario ya está activo.");

            var estadoAnterior = usuario.Estado;
            var fecha = DateTime.Now;

            usuario.Estado = "Activo";

            var restriccionesActivas = await _context.RestriccionesUsuario
                .Where(r => r.UsuarioId == usuarioId && r.EstadoRestriccion == "Activa")
                .ToListAsync();

            foreach (var restriccion in restriccionesActivas)
            {
                restriccion.EstadoRestriccion = "Finalizada";
                restriccion.FechaFin = fecha;
            }

            _context.RestriccionesUsuario.Add(new RestriccionesUsuario
            {
                UsuarioId = usuarioId,
                AdministradorId = administradorId,
                TipoAccion = "Habilitación",
                Mensaje = mensaje,
                FechaInicio = fecha,
                FechaFin = fecha,
                EstadoRestriccion = "Finalizada"
            });

            _context.AuditoriaAdministrativa.Add(new AuditoriaAdministrativa
            {
                AdministradorId = administradorId,
                UsuarioAfectadoId = usuarioId,
                TipoAccion = "Habilitación",
                MensajeRegistrado = mensaje,
                FechaHora = fecha
            });

            await CrearNotificacionAsync(
                usuarioId,
                "Habilitación",
                "Cuenta habilitada",
                mensaje,
                "RestriccionesUsuario",
                usuarioId);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new CambiarEstadoUsuarioResponseDto
            {
                UsuarioId = usuario.UsuarioId,
                NombreUsuario = usuario.NombreUsuario,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = usuario.Estado,
                Mensaje = mensaje,
                FechaAccion = fecha
            };
        }

        private async Task CancelarOrdenesActivasPorRestriccionAsync(int usuarioId, DateTime fecha)
        {
            var ordenes = await _context.OrdenesCompra
                .Include(o => o.ParMoneda)
                .Where(o =>
                    o.UsuarioId == usuarioId &&
                    o.CantidadPendiente > 0 &&
                    (o.Estado == "Activa" || o.Estado == "Parcialmente ejecutada"))
                .ToListAsync();

            foreach (var orden in ordenes)
            {
                var montoReembolso = orden.CantidadPendiente * orden.PrecioUnitario;

                await ReembolsarSaldoAsync(
                    usuarioId,
                    orden.ParMoneda.MonedaOrigenId,
                    montoReembolso,
                    "Reembolso",
                    "Orden de compra",
                    orden.OrdenCompraId,
                    fecha);

                orden.Estado = "Cancelada";
                orden.FechaCancelacion = fecha;
                orden.FechaActualizacion = fecha;

                await SincronizarOfertaEspejoCanceladaDesdeOrdenAsync(orden, fecha);

                _context.CancelacionesOrdenOferta.Add(new CancelacionesOrdenOferta
                {
                    UsuarioId = usuarioId,
                    TipoOperacion = "Orden de compra",
                    OrdenCompraId = orden.OrdenCompraId,
                    ParMonedaId = orden.ParMonedaId,
                    CantidadEjecutada = orden.CantidadObtenida,
                    CantidadCancelada = orden.CantidadPendiente,
                    MontoReembolsado = montoReembolso,
                    FechaCancelacion = fecha
                });

                _context.HistorialTransacciones.Add(new HistorialTransacciones
                {
                    UsuarioId = usuarioId,
                    TipoOperacion = "Cancelacion",
                    ReferenciaId = orden.OrdenCompraId,
                    ParMonedaId = orden.ParMonedaId,
                    FechaHora = fecha,
                    Estado = "Cancelada"
                });
            }
        }

        private async Task CancelarOfertasActivasPorRestriccionAsync(int usuarioId, DateTime fecha)
        {
            var ofertas = await _context.OfertasVenta
                .Include(o => o.ParMoneda)
                .Where(o =>
                    o.UsuarioId == usuarioId &&
                    o.CantidadPendiente > 0 &&
                    o.OrdenCompraEspejoId == null &&
                    (o.Estado == "Activa" || o.Estado == "Parcialmente ejecutada"))
                .ToListAsync();

            foreach (var oferta in ofertas)
            {
                await ReembolsarSaldoAsync(
                    usuarioId,
                    oferta.ParMoneda.MonedaDestinoId,
                    oferta.CantidadPendiente,
                    "Reembolso",
                    "Oferta de venta",
                    oferta.OfertaVentaId,
                    fecha);

                oferta.Estado = "Cancelada";
                oferta.FechaCancelacion = fecha;
                oferta.FechaActualizacion = fecha;

                _context.CancelacionesOrdenOferta.Add(new CancelacionesOrdenOferta
                {
                    UsuarioId = usuarioId,
                    TipoOperacion = "Oferta de venta",
                    OfertaVentaId = oferta.OfertaVentaId,
                    ParMonedaId = oferta.ParMonedaId,
                    CantidadEjecutada = oferta.CantidadVendida,
                    CantidadCancelada = oferta.CantidadPendiente,
                    MontoReembolsado = oferta.CantidadPendiente,
                    FechaCancelacion = fecha
                });

                _context.HistorialTransacciones.Add(new HistorialTransacciones
                {
                    UsuarioId = usuarioId,
                    TipoOperacion = "Cancelacion",
                    ReferenciaId = oferta.OfertaVentaId,
                    ParMonedaId = oferta.ParMonedaId,
                    FechaHora = fecha,
                    Estado = "Cancelada"
                });
            }
        }

        private async Task SincronizarOfertaEspejoCanceladaDesdeOrdenAsync(OrdenesCompra orden, DateTime fecha)
        {
            var ofertaEspejo = await _context.OfertasVenta
                .FirstOrDefaultAsync(o => o.OrdenCompraEspejoId == orden.OrdenCompraId);

            if (ofertaEspejo == null)
                return;

            ofertaEspejo.Estado = "Cancelada";
            ofertaEspejo.FechaCancelacion = fecha;
            ofertaEspejo.FechaActualizacion = fecha;
        }

        private async Task ReembolsarSaldoAsync(
            int usuarioId,
            int monedaId,
            decimal monto,
            string tipoMovimiento,
            string referenciaTipo,
            int referenciaId,
            DateTime fecha)
        {
            if (monto <= 0)
                return;

            var saldo = await _context.SaldosBilletera
                .Include(s => s.Billetera)
                .FirstOrDefaultAsync(s =>
                    s.Billetera.UsuarioId == usuarioId &&
                    s.MonedaId == monedaId);

            if (saldo == null)
                throw new InvalidOperationException("No se encontró el saldo de billetera para realizar el reembolso.");

            var saldoAnterior = saldo.SaldoDisponible;

            saldo.SaldoDisponible += monto;
            saldo.FechaActualizacion = fecha;

            _context.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = monedaId,
                TipoMovimiento = tipoMovimiento,
                Monto = monto,
                SaldoAnterior = saldoAnterior,
                SaldoPosterior = saldo.SaldoDisponible,
                FechaMovimiento = fecha,
                ReferenciaTipo = referenciaTipo,
                ReferenciaId = referenciaId
            });
        }

        private async Task CrearNotificacionAsync(
            int usuarioId,
            string tipoEvento,
            string asunto,
            string cuerpo,
            string referenciaTipo,
            int referenciaId)
        {
            var usuario = await _context.Usuarios.FirstAsync(u => u.UsuarioId == usuarioId);

            var tipo = await _context.TiposNotificacion
                .FirstOrDefaultAsync(t => t.Nombre == tipoEvento);

            _context.NotificacionesCorreo.Add(new NotificacionesCorreo
            {
                UsuarioId = usuarioId,
                CorreoDestino = usuario.CorreoElectronico,
                TipoEvento = tipoEvento,
                TipoNotificacionId = tipo?.TipoNotificacionId,
                Asunto = asunto,
                Cuerpo = cuerpo,
                EstadoEnvio = "Pendiente",
                FechaCreacion = DateTime.Now,
                ReferenciaTipo = referenciaTipo,
                ReferenciaId = referenciaId
            });
        }
    }
}