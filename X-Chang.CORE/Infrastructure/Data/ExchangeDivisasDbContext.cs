using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Infrastructure.Data;

public partial class ExchangeDivisasDbContext : DbContext
{
    public ExchangeDivisasDbContext()
    {
    }

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

            entity.HasOne(d => d.Moneda).WithMany(p => p.MovimientosBilletera)
                .HasForeignKey(d => d.MonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Movimient__Moned__04E4BC85");

            entity.HasOne(d => d.Usuario).WithMany(p => p.MovimientosBilletera)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Movimient__Usuar__03F0984C");
        });

        modelBuilder.Entity<NotificacionesCorreo>(entity =>
        {
            entity.HasKey(e => e.NotificacionId).HasName("PK__Notifica__BCC1202407EF8A8C");

            entity.HasIndex(e => new { e.EstadoEnvio, e.FechaCreacion }, "IX_NotificacionesCorreo_EstadoFecha");

            entity.Property(e => e.Asunto).HasMaxLength(150);
            entity.Property(e => e.CorreoDestino).HasMaxLength(100);
            entity.Property(e => e.EstadoEnvio)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ReferenciaTipo).HasMaxLength(40);
            entity.Property(e => e.TipoEvento).HasMaxLength(60);

            entity.HasOne(d => d.TipoNotificacion).WithMany(p => p.NotificacionesCorreo)
                .HasForeignKey(d => d.TipoNotificacionId)
                .HasConstraintName("FK_NotificacionesCorreo_TiposNotificacion");

            entity.HasOne(d => d.Usuario).WithMany(p => p.NotificacionesCorreo)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificac__Usuar__7EF6D905");
        });

        modelBuilder.Entity<OfertasVenta>(entity =>
        {
            entity.HasKey(e => e.OfertaVentaId).HasName("PK__OfertasV__038B819D08C60B3C");

            entity.HasIndex(e => new { e.ParMonedaId, e.Estado, e.PrecioUnitario }, "IX_OfertasVenta_ParEstadoPrecio");

            entity.HasIndex(e => new { e.UsuarioId, e.Estado, e.FechaCreacion }, "IX_OfertasVenta_UsuarioEstadoFecha").IsDescending(false, false, true);

            entity.Property(e => e.CantidadOriginal).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.CantidadPendiente).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.CantidadVendida).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("Activa");
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalEsperado).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalRecibido).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.ParMoneda).WithMany(p => p.OfertasVenta)
                .HasForeignKey(d => d.ParMonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OfertasVe__ParMo__3A4CA8FD");

            entity.HasOne(d => d.Usuario).WithMany(p => p.OfertasVenta)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OfertasVe__Usuar__395884C4");

            entity.HasOne(d => d.OrdenCompraEspejo)
                .WithMany(p => p.OfertasVentaEspejo)
                .HasForeignKey(d => d.OrdenCompraEspejoId)
                .HasConstraintName("FK_OfertasVenta_OrdenCompraEspejo");
        });

        modelBuilder.Entity<OperacionInmediataEjecuciones>(entity =>
        {
            entity.HasKey(e => e.OperacionInmediataEjecucionId).HasName("PK__Operacio__A40EB128DA13F5BA");

            entity.HasOne(d => d.Ejecucion).WithMany(p => p.OperacionInmediataEjecuciones)
                .HasForeignKey(d => d.EjecucionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Operacion__Ejecu__540C7B00");

            entity.HasOne(d => d.OperacionInmediata).WithMany(p => p.OperacionInmediataEjecuciones)
                .HasForeignKey(d => d.OperacionInmediataId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Operacion__Opera__531856C7");
        });

        modelBuilder.Entity<OperacionesInmediatas>(entity =>
        {
            entity.HasKey(e => e.OperacionInmediataId).HasName("PK__Operacio__D7DB43E41DBF16CD");

            entity.HasIndex(e => new { e.UsuarioId, e.FechaOperacion }, "IX_OperacionesInmediatas_UsuarioFecha").IsDescending(false, true);

            entity.Property(e => e.CantidadEjecutada).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.CantidadSolicitada).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Completada");
            entity.Property(e => e.FechaOperacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.MetodoEjecucion).HasMaxLength(20);
            entity.Property(e => e.PrecioMaximo).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.PrecioMinimo).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.PrecioPromedio).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TipoOperacion).HasMaxLength(20);
            entity.Property(e => e.TotalPagado).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalRecibido).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.OperacionPadre).WithMany(p => p.InverseOperacionPadre)
                .HasForeignKey(d => d.OperacionPadreId)
                .HasConstraintName("FK_OperacionesInmediatas_OperacionPadre");

            entity.HasOne(d => d.ParMoneda).WithMany(p => p.OperacionesInmediatas)
                .HasForeignKey(d => d.ParMonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Operacion__ParMo__503BEA1C");

            entity.HasOne(d => d.Usuario).WithMany(p => p.OperacionesInmediatas)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Operacion__Usuar__4F47C5E3");
        });

        modelBuilder.Entity<OrdenesCompra>(entity =>
        {
            entity.HasKey(e => e.OrdenCompraId).HasName("PK__OrdenesC__0B556E3670279B8E");

            entity.HasIndex(e => new { e.ParMonedaId, e.Estado, e.PrecioUnitario }, "IX_OrdenesCompra_ParEstadoPrecio").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.UsuarioId, e.Estado, e.FechaCreacion }, "IX_OrdenesCompra_UsuarioEstadoFecha").IsDescending(false, false, true);

            entity.Property(e => e.CantidadObtenida).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.CantidadOriginal).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.CantidadPendiente).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("Activa");
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalComprometido).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalEjecutado).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.ParMoneda).WithMany(p => p.OrdenesCompra)
                .HasForeignKey(d => d.ParMonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrdenesCo__ParMo__2B0A656D");

            entity.HasOne(d => d.Usuario).WithMany(p => p.OrdenesCompra)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrdenesCo__Usuar__2A164134");
        });

        modelBuilder.Entity<Paises>(entity =>
        {
            entity.HasKey(e => e.PaisId).HasName("PK__Paises__B501E18537AEB018");

            entity.HasIndex(e => e.Nombre, "UQ__Paises__75E3EFCFB97F6EBA").IsUnique();

            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(d => d.Moneda).WithMany(p => p.Paises)
                .HasForeignKey(d => d.MonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Paises__MonedaId__534D60F1");
        });

        modelBuilder.Entity<ParesMoneda>(entity =>
        {
            entity.HasKey(e => e.ParMonedaId).HasName("PK__ParesMon__E8663F7AAF75CECF");

            entity.HasIndex(e => e.MonedaDestinoId, "IX_ParesMoneda_Destino");

            entity.HasIndex(e => e.MonedaOrigenId, "IX_ParesMoneda_Origen");

            entity.HasIndex(e => new { e.MonedaOrigenId, e.MonedaDestinoId }, "IX_ParesMoneda_OrigenDestino");

            entity.HasIndex(e => new { e.MonedaOrigenId, e.MonedaDestinoId }, "UQ__ParesMon__19894B790287A61B").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);

            entity.HasOne(d => d.MonedaDestino).WithMany(p => p.ParesMonedaMonedaDestino)
                .HasForeignKey(d => d.MonedaDestinoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ParesMone__Moned__70DDC3D8");

            entity.HasOne(d => d.MonedaOrigen).WithMany(p => p.ParesMonedaMonedaOrigen)
                .HasForeignKey(d => d.MonedaOrigenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ParesMone__Moned__6FE99F9F");
        });

        modelBuilder.Entity<RestriccionesUsuario>(entity =>
        {
            entity.HasKey(e => e.RestriccionId).HasName("PK__Restricc__26D13DE6ABA547EB");

            entity.HasIndex(e => new { e.UsuarioId, e.EstadoRestriccion }, "IX_RestriccionesUsuario_UsuarioEstado");

            entity.Property(e => e.EstadoRestriccion)
                .HasMaxLength(20)
                .HasDefaultValue("Activa");
            entity.Property(e => e.FechaInicio).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Mensaje).HasMaxLength(300);
            entity.Property(e => e.TipoAccion).HasMaxLength(20);

            entity.HasOne(d => d.Administrador).WithMany(p => p.RestriccionesUsuarioAdministrador)
                .HasForeignKey(d => d.AdministradorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Restricci__Admin__1A9EF37A");

            entity.HasOne(d => d.Usuario).WithMany(p => p.RestriccionesUsuarioUsuario)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Restricci__Usuar__19AACF41");
        });

        modelBuilder.Entity<Retiros>(entity =>
        {
            entity.HasKey(e => e.RetiroId).HasName("PK__Retiros__992834D8D5B51F80");

            entity.HasIndex(e => new { e.UsuarioId, e.FechaRetiro }, "IX_Retiros_UsuarioFecha").IsDescending(false, true);

            entity.Property(e => e.ComisionAplicada).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Completada");
            entity.Property(e => e.FechaRetiro).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.MontoFinalRecibido).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.MontoRetirado).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.VoucherUrl).HasMaxLength(500);

            entity.HasOne(d => d.MetodoPago).WithMany(p => p.Retiros)
                .HasForeignKey(d => d.MetodoPagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Retiros__MetodoP__1BC821DD");

            entity.HasOne(d => d.Moneda).WithMany(p => p.Retiros)
                .HasForeignKey(d => d.MonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Retiros__MonedaI__1AD3FDA4");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Retiros)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Retiros__Usuario__19DFD96B");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PK__Roles__F92302F176C09BAB");

            entity.HasIndex(e => e.Nombre, "UQ__Roles__75E3EFCFB879AEE0").IsUnique();

            entity.Property(e => e.Nombre).HasMaxLength(30);
        });

        modelBuilder.Entity<RutaConversionSaltos>(entity =>
        {
            entity.HasKey(e => e.RutaConversionSaltoId).HasName("PK__RutaConv__E4D44EF3896B8299");

            entity.HasIndex(e => new { e.RutaConversionId, e.NumeroSalto }, "IX_RutaConversionSaltos_Ruta");

            entity.HasIndex(e => new { e.RutaConversionId, e.NumeroSalto }, "UQ__RutaConv__A841EEA92513F170").IsUnique();

            entity.Property(e => e.CantidadConvertida).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.PrecioMaximo).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.PrecioMinimo).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.PrecioPromedio).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.ResultadoObtenido).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.MonedaDestino).WithMany(p => p.RutaConversionSaltosMonedaDestino)
                .HasForeignKey(d => d.MonedaDestinoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RutaConve__Moned__719CDDE7");

            entity.HasOne(d => d.MonedaOrigen).WithMany(p => p.RutaConversionSaltosMonedaOrigen)
                .HasForeignKey(d => d.MonedaOrigenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RutaConve__Moned__70A8B9AE");

            entity.HasOne(d => d.OperacionInmediataHija).WithMany(p => p.RutaConversionSaltosOperacionInmediataHija)
                .HasForeignKey(d => d.OperacionInmediataHijaId)
                .HasConstraintName("FK_RutaConversionSaltos_OperacionHija");

            entity.HasOne(d => d.OperacionInmediata).WithMany(p => p.RutaConversionSaltosOperacionInmediata)
                .HasForeignKey(d => d.OperacionInmediataId)
                .HasConstraintName("FK__RutaConve__Opera__72910220");

            entity.HasOne(d => d.ParMoneda).WithMany(p => p.RutaConversionSaltos)
                .HasForeignKey(d => d.ParMonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RutaConve__ParMo__6FB49575");

            entity.HasOne(d => d.RutaConversion).WithMany(p => p.RutaConversionSaltos)
                .HasForeignKey(d => d.RutaConversionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RutaConve__RutaC__6EC0713C");
        });

        modelBuilder.Entity<RutasConversion>(entity =>
        {
            entity.HasKey(e => e.RutaConversionId).HasName("PK__RutasCon__E95E383970476D66");

            entity.Property(e => e.AhorroEstimado).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.GananciaEstimada).HasColumnType("decimal(28, 8)");
            entity.Property(e => e.TotalEstimado).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.BusquedaRuta).WithMany(p => p.RutasConversion)
                .HasForeignKey(d => d.BusquedaRutaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RutasConv__Busqu__65370702");

            entity.HasOne(d => d.MonedaFinal).WithMany(p => p.RutasConversionMonedaFinal)
                .HasForeignKey(d => d.MonedaFinalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RutasConv__Moned__681373AD");

            entity.HasOne(d => d.MonedaInicial).WithMany(p => p.RutasConversionMonedaInicial)
                .HasForeignKey(d => d.MonedaInicialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RutasConv__Moned__671F4F74");

            entity.HasOne(d => d.OperacionInmediata).WithMany(p => p.RutasConversion)
                .HasForeignKey(d => d.OperacionInmediataId)
                .HasConstraintName("FK__RutasConv__Opera__662B2B3B");
        });

        modelBuilder.Entity<SaldosBilletera>(entity =>
        {
            entity.HasKey(e => e.SaldoId).HasName("PK__SaldosBi__FF916F69E9AA2122");

            entity.HasIndex(e => new { e.BilleteraId, e.SaldoDisponible }, "IX_SaldosBilletera_BilleteraSaldo").IsDescending(false, true);

            entity.HasIndex(e => new { e.BilleteraId, e.MonedaId }, "UQ__SaldosBi__5F2DFF99DF454FF4").IsUnique();

            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.SaldoDisponible).HasColumnType("decimal(28, 8)");

            entity.HasOne(d => d.Billetera).WithMany(p => p.SaldosBilletera)
                .HasForeignKey(d => d.BilleteraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SaldosBil__Bille__693CA210");

            entity.HasOne(d => d.Moneda).WithMany(p => p.SaldosBilletera)
                .HasForeignKey(d => d.MonedaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SaldosBil__Moned__6A30C649");
        });

        modelBuilder.Entity<SesionesUsuario>(entity =>
        {
            entity.HasKey(e => e.SesionId).HasName("PK__Sesiones__52FD7C665E311931");

            entity.HasIndex(e => new { e.UsuarioId, e.Estado }, "IX_SesionesUsuario_UsuarioEstado");

            entity.HasIndex(e => e.TokenSesion, "UQ__Sesiones__567B1115E3B3DAE8").IsUnique();

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Activa");
            entity.Property(e => e.FechaInicio).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TokenSesion).HasMaxLength(500);

            entity.HasOne(d => d.Usuario).WithMany(p => p.SesionesUsuario)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SesionesU__Usuar__214BF109");
        });

        modelBuilder.Entity<TiposNotificacion>(entity =>
        {
            entity.HasKey(e => e.TipoNotificacionId).HasName("PK__TiposNot__BE05838D63AA113B");

            entity.HasIndex(e => e.Nombre, "UQ__TiposNot__75E3EFCFCFEE138F").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.Nombre).HasMaxLength(80);
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__Usuarios__2B3DE7B810294C22");

            entity.HasIndex(e => e.CorreoElectronico, "IX_Usuarios_Correo");

            entity.HasIndex(e => e.NombreUsuario, "IX_Usuarios_NombreUsuario");

            entity.HasIndex(e => e.CorreoElectronico, "UQ__Usuarios__531402F3606C3BB4").IsUnique();

            entity.HasIndex(e => e.NombreUsuario, "UQ__Usuarios__6B0F5AE0C339C1FA").IsUnique();

            entity.Property(e => e.CorreoElectronico).HasMaxLength(100);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Activo");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.NombreUsuario).HasMaxLength(30);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.TemaVisual)
                .HasMaxLength(10)
                .HasDefaultValue("Claro");

            entity.HasOne(d => d.Pais).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.PaisId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios__PaisId__5DCAEF64");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios__RolId__5CD6CB2B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
