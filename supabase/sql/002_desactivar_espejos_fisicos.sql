-- Desactiva filas espejo físicas creadas por versiones anteriores del backend.
-- La versión actual interpreta la simetría al leer el libro de órdenes, sin duplicar liquidez.
-- Ejecutar una sola vez en Supabase SQL Editor después de desplegar el backend actualizado.

begin;

-- Ofertas espejo creadas desde órdenes de compra.
-- No tienen movimiento de reserva propio de OfertaVenta; solo duplicaban visualmente la orden original.
update public.ofertasventa ov
set estado = 'Cancelada',
    cantidadpendiente = 0,
    fechaactualizacion = now(),
    fechacancelacion = coalesce(ov.fechacancelacion, now())
where ov.ordencompraespejoid is not null
  and ov.estado in ('Activa', 'Parcialmente ejecutada')
  and not exists (
      select 1
      from public.movimientosbilletera mb
      where mb.referenciaid = ov.ofertaventaid
        and mb.referenciatipo in ('OfertaVenta', 'ofertasventa')
  );

-- Órdenes espejo creadas desde ofertas de venta.
-- No tienen movimiento de reserva propio de OrdenCompra; solo duplicaban visualmente la oferta original.
update public.ordenescompra oc
set estado = 'Cancelada',
    cantidadpendiente = 0,
    fechaactualizacion = now(),
    fechacancelacion = coalesce(oc.fechacancelacion, now())
where oc.estado in ('Activa', 'Parcialmente ejecutada')
  and exists (
      select 1
      from public.ofertasventa ov
      where ov.ordencompraespejoid = oc.ordencompraid
  )
  and not exists (
      select 1
      from public.movimientosbilletera mb
      where mb.referenciaid = oc.ordencompraid
        and mb.referenciatipo in ('OrdenCompra', 'ordenescompra')
  );

commit;
