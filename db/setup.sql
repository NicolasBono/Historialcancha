/*
  Setup inicial de la base. Se ejecuta UNA sola vez, con un login sysadmin
  (autenticación de Windows), porque el usuario de la aplicación no tiene
  permiso para crear bases —y no debería tenerlo—.

  Este script NO crea tablas: de eso se encargan las migraciones de EF Core.
  Acá sólo se crea la base y el usuario que la aplicación va a usar.

  Uso (la contraseña se pasa por parámetro, no se versiona):

    sqlcmd -S ".\SQLEXPRESS" -E -C -i db/setup.sql -v LoginApp="admin" LoginPassword="tu_password"
*/

:on error exit

IF DB_ID('HistorialCancha') IS NULL
BEGIN
    PRINT 'Creando base HistorialCancha...';
    CREATE DATABASE HistorialCancha;
END
ELSE
    PRINT 'La base HistorialCancha ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = '$(LoginApp)')
BEGIN
    PRINT 'Creando login $(LoginApp)...';
    CREATE LOGIN [$(LoginApp)] WITH PASSWORD = '$(LoginPassword)', CHECK_POLICY = OFF;
END
ELSE
    PRINT 'El login $(LoginApp) ya existe.';
GO

USE HistorialCancha;
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = '$(LoginApp)')
BEGIN
    PRINT 'Mapeando el usuario $(LoginApp) en HistorialCancha...';
    CREATE USER [$(LoginApp)] FOR LOGIN [$(LoginApp)];
END
GO

-- db_owner sobre esta base y nada más: alcanza para aplicar migraciones
-- y para operar, sin darle permisos de servidor.
ALTER ROLE db_owner ADD MEMBER [$(LoginApp)];
GO

PRINT 'Setup completo. Ahora corré: dotnet ef database update';
GO
