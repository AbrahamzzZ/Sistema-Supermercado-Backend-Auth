-- =============================================================
-- Auth · V1_02 · Procedimientos de USUARIO
-- =============================================================

CREATE OR ALTER PROCEDURE PA_LISTA_USUARIO
AS
BEGIN
    SET NOCOUNT ON;

    SELECT U.ID_USUARIO, U.CODIGO, U.NOMBRE_COMPLETO, U.CORREO_ELECTRONICO, R.ID_ROL, R.NOMBRE AS NOMBRE_ROL, U.ESTADO, U.FECHA_CREACION
    FROM USUARIO U
    INNER JOIN ROL R ON U.ID_ROL = R.ID_ROL
    WHERE U.ELIMINADO = 0;
END;
GO

CREATE OR ALTER PROCEDURE PA_LISTA_USUARIO_PAGINACION(
    @PageNumber INT = 1,
    @PageSize INT = 10
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalCount INT;
    SELECT @TotalCount = COUNT(*)
    FROM USUARIO U
    INNER JOIN ROL R ON U.ID_ROL = R.ID_ROL
    WHERE U.ELIMINADO = 0;

    -- El orden de las columnas importa: el repositorio las lee por posición
    SELECT U.ID_USUARIO, U.CODIGO, U.NOMBRE_COMPLETO, U.CORREO_ELECTRONICO, R.ID_ROL, R.NOMBRE AS NOMBRE_ROL, U.ESTADO, U.FECHA_CREACION
    FROM USUARIO U
    INNER JOIN ROL R ON U.ID_ROL = R.ID_ROL
    WHERE U.ELIMINADO = 0
    ORDER BY U.ID_USUARIO OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT @TotalCount AS TotalCount;
END;
GO

CREATE OR ALTER PROCEDURE PA_OBTENER_USUARIO(
    @Id_Usuario INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT U.ID_USUARIO, U.CODIGO, U.NOMBRE_COMPLETO, U.CORREO_ELECTRONICO, R.ID_ROL, R.NOMBRE AS NOMBRE_ROL, U.ESTADO, U.FECHA_CREACION
    FROM USUARIO U
    INNER JOIN ROL R ON U.ID_ROL = R.ID_ROL
    WHERE U.ID_USUARIO = @Id_Usuario AND U.ELIMINADO = 0;
END;
GO

-- Devuelve el usuario con su hash para que la API verifique la clave (BCrypt).
-- Antes recibía también @Clave y comparaba el SHA-256 en SQL.
CREATE OR ALTER PROCEDURE PA_INICIAR_SESION(
    @Correo_Electronico NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT U.ID_USUARIO, U.CODIGO, U.NOMBRE_COMPLETO, U.CORREO_ELECTRONICO, R.ID_ROL, R.NOMBRE AS NOMBRE_ROL, U.ESTADO, U.FECHA_CREACION, U.CLAVE
    FROM USUARIO U
    INNER JOIN ROL R ON U.ID_ROL = R.ID_ROL
    WHERE U.CORREO_ELECTRONICO = @Correo_Electronico AND U.ELIMINADO = 0;
END;
GO

-- Usado al iniciar sesión para migrar claves con hash antiguo (SHA-256) a BCrypt.
CREATE OR ALTER PROCEDURE PA_ACTUALIZAR_CLAVE(
    @Id_Usuario INT,
    @Clave NVARCHAR(300)
)
AS
BEGIN
    UPDATE USUARIO SET CLAVE = @Clave
    WHERE ID_USUARIO = @Id_Usuario AND ELIMINADO = 0;
END;
GO

CREATE OR ALTER PROCEDURE PA_REGISTRAR_USUARIO(
    @Codigo VARCHAR(10),
    @Nombre_Completo VARCHAR(70),
    @Correo_Electronico NVARCHAR(50),
    @Clave NVARCHAR(300),
    @Id_Rol INT,
    @Estado BIT,
    @Id_Usuario_Creacion INT = NULL
)
AS
BEGIN
    INSERT INTO USUARIO (CODIGO, NOMBRE_COMPLETO, CORREO_ELECTRONICO, CLAVE, ID_ROL, ESTADO, ID_USUARIO_CREACION, FECHA_CREACION)
    VALUES (@Codigo, @Nombre_Completo, @Correo_Electronico, @Clave, @Id_Rol, @Estado, @Id_Usuario_Creacion, SYSUTCDATETIME());
END;
GO

CREATE OR ALTER PROCEDURE PA_EDITAR_USUARIO(
    @Id_Usuario INT,
    @Nombre_Completo VARCHAR(70),
    @Correo_Electronico NVARCHAR(50),
    @Clave NVARCHAR(300),
    @Id_Rol INT,
    @Estado BIT,
    @Id_Usuario_Modificacion INT = NULL
)
AS
BEGIN
    UPDATE USUARIO
    SET NOMBRE_COMPLETO = @Nombre_Completo,
        CORREO_ELECTRONICO = @Correo_Electronico,
        CLAVE = @Clave,
        ID_ROL = @Id_Rol,
        ESTADO = @Estado,
        ID_USUARIO_MODIFICACION = @Id_Usuario_Modificacion,
        FECHA_MODIFICACION = SYSUTCDATETIME()
    WHERE ID_USUARIO = @Id_Usuario AND ELIMINADO = 0;
END;
GO

-- Borrado lógico: el registro se conserva (otros servicios guardan este ID_USUARIO).
CREATE OR ALTER PROCEDURE PA_ELIMINAR_USUARIO(
    @Id_Usuario INT,
    @Id_Usuario_Modificacion INT = NULL
)
AS
BEGIN
    UPDATE USUARIO
    SET ELIMINADO = 1,
        ID_USUARIO_MODIFICACION = @Id_Usuario_Modificacion,
        FECHA_MODIFICACION = SYSUTCDATETIME()
    WHERE ID_USUARIO = @Id_Usuario AND ELIMINADO = 0;
END;
GO
