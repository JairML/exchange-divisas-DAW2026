using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Shared
{
    /// <summary>
    /// Utilitario compartido para aplicar un movimiento sobre el saldo de la billetera
    /// de un usuario. Lo usan tanto el depósito (US-007, abono) como la cancelación
    /// (US-022, reembolso). NO llama a SaveChanges: deja la persistencia al método que
    /// orquesta la transacción, de modo que todo el cambio sea atómico.
    /// </summary>
    internal static class MovimientoBilleteraHelper
    {
        /// <summary>
        /// Suma <paramref name="delta"/> al saldo de la moneda indicada (puede ser negativo)
        /// y registra el movimiento correspondiente. Devuelve el saldo anterior y el posterior.
        /// </summary>
        public static async Task<(decimal anterior, decimal posterior)> Aplicar(
            ExchangeDivisasDbContext ctx,
            int usuarioId,
            int monedaId,
            decimal delta,
            string tipoMovimiento,
            string? referenciaTipo,
            int? referenciaId)
        {
            var ahora = DateTime.Now;

            // Billetera del usuario (se crea si no existiera).
            var billetera = await ctx.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);
            if (billetera == null)
            {
                billetera = new Billeteras
                {
                    UsuarioId = usuarioId,
                    FechaCreacion = ahora
                };
                ctx.Billeteras.Add(billetera);
            }

            // Saldo de la moneda (se crea en 0 si no existiera).
            var saldo = await ctx.SaldosBilletera
                .FirstOrDefaultAsync(s => s.BilleteraId == billetera.BilleteraId && s.MonedaId == monedaId);
            if (saldo == null)
            {
                saldo = new SaldosBilletera
                {
                    Billetera = billetera,
                    MonedaId = monedaId,
                    SaldoDisponible = 0m,
                    FechaActualizacion = ahora
                };
                ctx.SaldosBilletera.Add(saldo);
            }

            var anterior = saldo.SaldoDisponible;
            var posterior = anterior + delta;

            saldo.SaldoDisponible = posterior;
            saldo.FechaActualizacion = ahora;

            ctx.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = monedaId,
                TipoMovimiento = tipoMovimiento,
                Monto = delta,
                SaldoAnterior = anterior,
                SaldoPosterior = posterior,
                FechaMovimiento = ahora,
                ReferenciaTipo = referenciaTipo,
                ReferenciaId = referenciaId
            });

            return (anterior, posterior);
        }
    }
}
