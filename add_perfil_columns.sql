-- Agrega las columnas Telefono y FotoPerfilUrl a la tabla Usuarios.
-- Ejecutar una sola vez contra ExchangeDivisasDB.

ALTER TABLE Usuarios
    ADD Telefono     NVARCHAR(20)  NULL,
        FotoPerfilUrl NVARCHAR(500) NULL;
GO
