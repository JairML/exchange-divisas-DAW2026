-- Siembra precios históricos para todos los pares activos.
-- Genera un punto cada 4 horas durante los últimos 14 meses.
-- Ejecutar UNA SOLA VEZ en Supabase SQL Editor (Studio > SQL Editor > New query).

DO $$
DECLARE
  v_par   RECORD;
  v_base  NUMERIC;
  v_sprd  NUMERIC;
BEGIN
  FOR v_par IN
    SELECT
      pm.parmonedaid,
      mo."CodigoISO" AS origen,
      md."CodigoISO" AS destino
    FROM   public.paresmoneda pm
    JOIN   public.monedas mo ON pm.monedaorigenid = mo.monedaid
    JOIN   public.monedas md ON pm.monedadestinoid = md.monedaid
    WHERE  pm.activo = true
  LOOP
    -- Tasa de referencia por par (promedio histórico real 2025-2026)
    v_base := CASE
      WHEN v_par.origen = 'USD' AND v_par.destino = 'EUR' THEN 0.9210
      WHEN v_par.origen = 'EUR' AND v_par.destino = 'USD' THEN 1.0858
      WHEN v_par.origen = 'USD' AND v_par.destino = 'GBP' THEN 0.7920
      WHEN v_par.origen = 'GBP' AND v_par.destino = 'USD' THEN 1.2626
      WHEN v_par.origen = 'USD' AND v_par.destino = 'JPY' THEN 149.20
      WHEN v_par.origen = 'JPY' AND v_par.destino = 'USD' THEN 0.006700
      WHEN v_par.origen = 'USD' AND v_par.destino = 'PEN' THEN 3.7400
      WHEN v_par.origen = 'PEN' AND v_par.destino = 'USD' THEN 0.26738
      WHEN v_par.origen = 'USD' AND v_par.destino = 'CHF' THEN 0.9070
      WHEN v_par.origen = 'CHF' AND v_par.destino = 'USD' THEN 1.1025
      WHEN v_par.origen = 'USD' AND v_par.destino = 'CAD' THEN 1.3590
      WHEN v_par.origen = 'CAD' AND v_par.destino = 'USD' THEN 0.73583
      WHEN v_par.origen = 'USD' AND v_par.destino = 'AUD' THEN 1.5340
      WHEN v_par.origen = 'AUD' AND v_par.destino = 'USD' THEN 0.65191
      WHEN v_par.origen = 'USD' AND v_par.destino = 'MXN' THEN 17.250
      WHEN v_par.origen = 'MXN' AND v_par.destino = 'USD' THEN 0.057971
      WHEN v_par.origen = 'USD' AND v_par.destino = 'BRL' THEN 4.9300
      WHEN v_par.origen = 'BRL' AND v_par.destino = 'USD' THEN 0.20284
      WHEN v_par.origen = 'USD' AND v_par.destino = 'COP' THEN 3960.0
      WHEN v_par.origen = 'COP' AND v_par.destino = 'USD' THEN 0.000253
      WHEN v_par.origen = 'USD' AND v_par.destino = 'CLP' THEN 935.00
      WHEN v_par.origen = 'CLP' AND v_par.destino = 'USD' THEN 0.001070
      WHEN v_par.origen = 'EUR' AND v_par.destino = 'GBP' THEN 0.8598
      WHEN v_par.origen = 'GBP' AND v_par.destino = 'EUR' THEN 1.1630
      WHEN v_par.origen = 'EUR' AND v_par.destino = 'PEN' THEN 4.0600
      WHEN v_par.origen = 'PEN' AND v_par.destino = 'EUR' THEN 0.24631
      WHEN v_par.origen = 'EUR' AND v_par.destino = 'JPY' THEN 161.80
      WHEN v_par.origen = 'JPY' AND v_par.destino = 'EUR' THEN 0.006180
      WHEN v_par.origen = 'GBP' AND v_par.destino = 'JPY' THEN 188.50
      WHEN v_par.origen = 'JPY' AND v_par.destino = 'GBP' THEN 0.005306
      WHEN v_par.origen = 'PEN' AND v_par.destino = 'COP' THEN 1056.0
      WHEN v_par.origen = 'COP' AND v_par.destino = 'PEN' THEN 0.000947
      ELSE 1.0000
    END;

    -- Spread proporcional al precio (±0.1 %)
    v_sprd := v_base * 0.0010;

    INSERT INTO public.historicopreciospar (
      parmonedaid,
      mayorpreciocompra,
      menorprecioventa,
      volumencompra,
      volumenventa,
      fecharegistro,
      snapshotminuto
    )
    SELECT
      v_par.parmonedaid,
      -- bid: precio compra (menor)
      ROUND(GREATEST(mid - v_sprd / 2.0, 0.000001), 8),
      -- ask: precio venta (mayor)
      ROUND(mid + v_sprd / 2.0, 8),
      ROUND((300 + random() * 9700)::NUMERIC, 4),
      ROUND((250 + random() * 8750)::NUMERIC, 4),
      ts,
      ts
    FROM (
      SELECT
        ts,
        -- Precio medio con tendencia sinusoidal + ruido aleatorio
        (
          v_base
          * (1.0 + 0.035 * SIN(EXTRACT(EPOCH FROM ts) / 86400.0 / 44.0))
          * (1.0 + 0.012 * SIN(EXTRACT(EPOCH FROM ts) / 86400.0 / 7.5))
          + (random() - 0.5) * v_base * 0.004
        )::NUMERIC AS mid
      FROM generate_series(
        (NOW() AT TIME ZONE 'UTC') - INTERVAL '14 months',
        (NOW() AT TIME ZONE 'UTC'),
        INTERVAL '4 hours'
      ) AS ts
    ) sub
    ON CONFLICT ON CONSTRAINT uq_historicopreciospar_par_minuto DO NOTHING;

    RAISE NOTICE 'Seeded: % → % (id=%)', v_par.origen, v_par.destino, v_par.parmonedaid;
  END LOOP;

  RAISE NOTICE 'Seed completado.';
END;
$$;
