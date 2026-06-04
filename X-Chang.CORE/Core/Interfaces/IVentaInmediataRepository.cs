using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.DTOs.VentaInmediata;
using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IVentaInmediataRepository
    {
        Task<ParesMoneda?> ObtenerParMonedaAsync(int parMonedaId);

        Task<List<ParesMoneda>> ObtenerParesActivosAsync();

        Task<List<OrdenesCompra>> ObtenerOrdenesCompraActivasAsync(
            int parMonedaId);

        Task<decimal> ObtenerSaldoDisponibleAsync(
            int usuarioId,
            int monedaId);

        Task<BusquedasRuta> CrearBusquedaRutaAsync(
            int usuarioId,
            int parMonedaId,
            decimal cantidadSolicitada,
            int maxSaltos,
            int tiempoEstimadoMs);

        Task<BusquedasRuta?> ObtenerBusquedaRutaAsync(
            int busquedaRutaId,
            int usuarioId);

        Task<BusquedasRuta?> ObtenerBusquedaRutaConSaltosAsync(
            int busquedaRutaId,
            int usuarioId);

        Task FinalizarBusquedaRutaSinResultadoAsync(
            int busquedaRutaId);

        Task CancelarBusquedaRutaAsync(
            int busquedaRutaId);

        Task GuardarResultadoRutaAsync(
            int busquedaRutaId,
            ResultadoBusquedaRutaVentaDto resultado);

        Task<VentaInmediataResponseDto> EjecutarVentaInmediataNormalAsync(
            int usuarioId,
            int parMonedaId,
            decimal cantidadAVender,
            bool venderCantidadDisponible);

        Task<VentaInmediataResponseDto> EjecutarVentaInmediataPorRutaAsync(
            int usuarioId,
            int busquedaRutaId);
    }
}