using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.Billetera;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;
using X_Chang.CORE.Infrastructure.Shared;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class RetirosRepository : IRetirosRepository
{
    private readonly ExchangeDivisasDbContext _context;
    public RetirosRepository(ExchangeDivisasDbContext context) => _context = context;

    private static DetalleRetiroDto Map(Retiros r) => new()
    {
        RetiroId = r.RetiroId,
        MonedaId = r.MonedaId,
        CodigoISO = r.Moneda.CodigoIso,
        NombreMoneda = r.Moneda.Nombre,
        MetodoPagoId = r.MetodoPagoId,
        NombreMetodoPago = r.MetodoPago.Nombre,
        MontoRetirado = r.MontoRetirado,
        ComisionAplicada = r.ComisionAplicada,
        MontoFinalRecibido = r.MontoFinalRecibido,
        Estado = r.Estado,
        VoucherUrl = r.VoucherUrl,
        FechaRetiro = r.FechaRetiro
    };

    public async Task<List<DetalleRetiroDto>> ListarAsync(int usuarioId, int pagina, int tamano) =>
        (await _context.Retiros
            .Include(r => r.Moneda)
            .Include(r => r.MetodoPago)
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.FechaRetiro)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync())
            .Select(Map).ToList();

    public Task<int> ContarAsync(int usuarioId) =>
        _context.Retiros.CountAsync(r => r.UsuarioId == usuarioId);

    public async Task<DetalleRetiroDto?> ObtenerDetalleAsync(int usuarioId, int retiroId)
    {
        var r = await _context.Retiros
            .Include(r => r.Moneda)
            .Include(r => r.MetodoPago)
            .FirstOrDefaultAsync(r => r.RetiroId == retiroId && r.UsuarioId == usuarioId);

        return r == null ? null : Map(r);
    }

    public async Task<DetalleRetiroDto> RegistrarRetiroAsync(int usuarioId, RetirarDto dto)
    {
        // 1) Validaciones previas a la transacción
        var moneda = await _context.Monedas.FindAsync(dto.MonedaId)
            ?? throw new ArgumentException("Moneda no encontrada.");

        var metodo = await _context.MetodosPago.FindAsync(dto.MetodoPagoId)
            ?? throw new ArgumentException("Método de pago no encontrado.");

        var billetera = await _context.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("El usuario no tiene billetera.");

        var saldo = await _context.SaldosBilletera
            .FirstOrDefaultAsync(s => s.BilleteraId == billetera.BilleteraId && s.MonedaId == dto.MonedaId);

        if (saldo == null || saldo.SaldoDisponible < dto.MontoRetirado)
            throw new InvalidOperationException("Saldo insuficiente.");

        // 2) Cálculo de comisión
        var comision = metodo.ComisionFija + (dto.MontoRetirado * metodo.ComisionPorcentaje / 100m);
        var montoFinal = dto.MontoRetirado - comision;

        var usuario = await _context.Usuarios.FindAsync(usuarioId)!;
        var ahora = DateTime.Now;

        using var tx = await _context.Database.BeginTransactionAsync();

        // 3) Registrar el retiro
        var retiro = new Retiros
        {
            UsuarioId = usuarioId,
            MonedaId = dto.MonedaId,
            MetodoPagoId = dto.MetodoPagoId,
            MontoRetirado = dto.MontoRetirado,
            ComisionAplicada = comision,
            MontoFinalRecibido = montoFinal,
            Estado = "Completada",
            FechaRetiro = ahora
        };
        _context.Retiros.Add(retiro);
        await _context.SaveChangesAsync();

        var voucherUrl = dto.VoucherUrl ?? $"https://X_Chang.local/vouchers/retiro-{retiro.RetiroId}.pdf";
        retiro.VoucherUrl = voucherUrl;

        // 4) Debitar saldo (delta negativo)
        await MovimientoBilleteraHelper.Aplicar(
            _context, usuarioId, dto.MonedaId, -dto.MontoRetirado, "Retiro", "Retiro", retiro.RetiroId);

        // 5) Historial de transacciones
        _context.HistorialTransacciones.Add(new HistorialTransacciones
        {
            UsuarioId = usuarioId,
            TipoOperacion = "Retiro",
            ReferenciaId = retiro.RetiroId,
            MonedaId = dto.MonedaId,
            FechaHora = ahora,
            Estado = "Completada"
        });

        // 6) Notificación de correo
        var tipoNotifId = await _context.TiposNotificacion
            .Where(t => t.Nombre == "Retiro")
            .Select(t => (int?)t.TipoNotificacionId)
            .FirstOrDefaultAsync();

        var notif = new NotificacionesCorreo
        {
            UsuarioId = usuarioId,
            CorreoDestino = usuario!.CorreoElectronico,
            TipoEvento = "Retiro",
            TipoNotificacionId = tipoNotifId,
            Asunto = "Retiro completado",
            Cuerpo = $"Tu retiro de {dto.MontoRetirado} {moneda.CodigoIso} fue procesado. Recibirás {montoFinal} {moneda.CodigoIso}.",
            EstadoEnvio = "Pendiente",
            FechaCreacion = ahora,
            ReferenciaTipo = "Retiro",
            ReferenciaId = retiro.RetiroId
        };
        _context.NotificacionesCorreo.Add(notif);
        await _context.SaveChangesAsync();

        _context.AdjuntosCorreo.Add(new AdjuntosCorreo
        {
            NotificacionId = notif.NotificacionId,
            NombreArchivo = $"voucher-retiro-{retiro.RetiroId}.pdf",
            UrlArchivo = voucherUrl,
            TipoContenido = "application/pdf"
        });
        await _context.SaveChangesAsync();

        await tx.CommitAsync();

        return new DetalleRetiroDto
        {
            RetiroId = retiro.RetiroId,
            MonedaId = moneda.MonedaId,
            CodigoISO = moneda.CodigoIso,
            NombreMoneda = moneda.Nombre,
            MetodoPagoId = metodo.MetodoPagoId,
            NombreMetodoPago = metodo.Nombre,
            MontoRetirado = retiro.MontoRetirado,
            ComisionAplicada = retiro.ComisionAplicada,
            MontoFinalRecibido = retiro.MontoFinalRecibido,
            Estado = retiro.Estado,
            VoucherUrl = retiro.VoucherUrl,
            FechaRetiro = retiro.FechaRetiro
        };
    }
}
