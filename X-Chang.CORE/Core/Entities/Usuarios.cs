using System;
using System.Collections.Generic;

namespace X_Chang.CORE.Core.Entities;

public partial class Usuarios
{
    public int UsuarioId { get; set; }

    public int RolId { get; set; }

    public int PaisId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string CorreoElectronico { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string TemaVisual { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaUltimoAcceso { get; set; }

    public string? Telefono { get; set; }

    public string? TipoDocumento { get; set; }

    public string? NumeroDocumento { get; set; }

    public string? FotoUrl { get; set; }

    public virtual ICollection<AccesosUsuario> AccesosUsuario { get; set; } = new List<AccesosUsuario>();

    public virtual ICollection<AuditoriaAdministrativa> AuditoriaAdministrativaAdministrador { get; set; } = new List<AuditoriaAdministrativa>();

    public virtual ICollection<AuditoriaAdministrativa> AuditoriaAdministrativaUsuarioAfectado { get; set; } = new List<AuditoriaAdministrativa>();

    public virtual Billeteras? Billeteras { get; set; }

    public virtual ICollection<BusquedasRuta> BusquedasRuta { get; set; } = new List<BusquedasRuta>();

    public virtual ICollection<CancelacionesOrdenOferta> CancelacionesOrdenOferta { get; set; } = new List<CancelacionesOrdenOferta>();

    public virtual ICollection<Depositos> Depositos { get; set; } = new List<Depositos>();

    public virtual ICollection<EjecucionesOrden> EjecucionesOrdenComprador { get; set; } = new List<EjecucionesOrden>();

    public virtual ICollection<EjecucionesOrden> EjecucionesOrdenVendedor { get; set; } = new List<EjecucionesOrden>();

    public virtual ICollection<ExportacionesReporte> ExportacionesReporte { get; set; } = new List<ExportacionesReporte>();

    public virtual ICollection<HistorialTransacciones> HistorialTransacciones { get; set; } = new List<HistorialTransacciones>();

    public virtual ICollection<MovimientosBilletera> MovimientosBilletera { get; set; } = new List<MovimientosBilletera>();

    public virtual ICollection<NotificacionesCorreo> NotificacionesCorreo { get; set; } = new List<NotificacionesCorreo>();

    public virtual ICollection<OfertasVenta> OfertasVenta { get; set; } = new List<OfertasVenta>();

    public virtual ICollection<OperacionesInmediatas> OperacionesInmediatas { get; set; } = new List<OperacionesInmediatas>();

    public virtual ICollection<OrdenesCompra> OrdenesCompra { get; set; } = new List<OrdenesCompra>();

    public virtual Paises Pais { get; set; } = null!;

    public virtual ICollection<RestriccionesUsuario> RestriccionesUsuarioAdministrador { get; set; } = new List<RestriccionesUsuario>();

    public virtual ICollection<RestriccionesUsuario> RestriccionesUsuarioUsuario { get; set; } = new List<RestriccionesUsuario>();

    public virtual ICollection<Retiros> Retiros { get; set; } = new List<Retiros>();

    public virtual Roles Rol { get; set; } = null!;

    public virtual ICollection<SesionesUsuario> SesionesUsuario { get; set; } = new List<SesionesUsuario>();
}
