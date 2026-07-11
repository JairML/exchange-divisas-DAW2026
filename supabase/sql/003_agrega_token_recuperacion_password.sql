-- Agrega las columnas usadas por el flujo de "recuperar contraseña" (forgot/reset password).
-- Ejecutar una sola vez en Supabase SQL Editor después de desplegar el backend actualizado.

begin;

alter table public.usuarios
  add column if not exists tokenrecuperacion text null,
  add column if not exists tokenrecuperacionexpira timestamptz null;

commit;
