-- Ejecutar UNA VEZ en Supabase SQL Editor (Studio > SQL Editor > New query)
-- Crea la tabla de auditoría de estados de órdenes y ofertas.

CREATE TABLE IF NOT EXISTS public.logestadosoperacion (
    logid         SERIAL PRIMARY KEY,
    tipooperacion VARCHAR(20)    NOT NULL,
    referenciaid  INTEGER        NOT NULL,
    estadoanterior VARCHAR(30),
    estadonuevo   VARCHAR(30)    NOT NULL,
    fechacambio   TIMESTAMP      NOT NULL,
    motivo        VARCHAR(100),
    cantidadafectada DECIMAL(28, 8)
);

CREATE INDEX IF NOT EXISTS ix_logestados_tipo_ref
    ON public.logestadosoperacion (tipooperacion, referenciaid);

RAISE NOTICE 'Tabla logestadosoperacion creada correctamente.';
