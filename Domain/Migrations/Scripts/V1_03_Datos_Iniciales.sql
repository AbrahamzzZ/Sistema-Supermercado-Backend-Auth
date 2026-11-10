-- =============================================================
-- Auth · V1_03 · Datos iniciales
-- =============================================================

-- ROLES
SET IDENTITY_INSERT ROL ON;

INSERT INTO ROL (ID_ROL, NOMBRE)
SELECT v.ID_ROL, v.NOMBRE
FROM (VALUES (1, 'Administrador'), (2, 'Empleado')) AS v (ID_ROL, NOMBRE)
WHERE NOT EXISTS (SELECT 1 FROM ROL r WHERE r.ID_ROL = v.ID_ROL);

SET IDENTITY_INSERT ROL OFF;
GO

-- MENÚS
SET IDENTITY_INSERT MENU ON;

INSERT INTO MENU (ID_MENU, NOMBRE_MENU, CODIGO_MENU, NOMBRE_ICONO)
SELECT v.ID_MENU, v.NOMBRE_MENU, v.CODIGO_MENU, v.NOMBRE_ICONO
FROM (VALUES
    (1,  'Usuarios',      '001', 'person'),
    (2,  'Compra',        '002', 'local_grocery_store'),
    (3,  'Venta',         '003', 'attach_money'),
    (4,  'Productos',     '004', 'shopping_basket'),
    (5,  'Categorias',    '005', 'storage'),
    (6,  'Clientes',      '006', 'group'),
    (7,  'Proveedores',   '007', 'inventory'),
    (8,  'Transportista', '008', 'local_shipping'),
    (9,  'Ofertas',       '009', 'more'),
    (10, 'Sucursales',    '010', 'location_city'),
    (11, 'Log',           '011', 'insert_drive_file')
) AS v (ID_MENU, NOMBRE_MENU, CODIGO_MENU, NOMBRE_ICONO)
WHERE NOT EXISTS (SELECT 1 FROM MENU m WHERE m.ID_MENU = v.ID_MENU);

SET IDENTITY_INSERT MENU OFF;
GO

-- PERMISOS: Administrador (1) tiene todos los menús; Empleado (2): Compras, Ventas, Productos, Categorías y Ofertas
INSERT INTO PERMISO (ID_ROL, ID_MENU)
SELECT v.ID_ROL, v.ID_MENU
FROM (VALUES
    (1, 1), (1, 2), (1, 3), (1, 4), (1, 5), (1, 6), (1, 7), (1, 8), (1, 9), (1, 10), (1, 11), (1, 12),
    (2, 2), (2, 3), (2, 4), (2, 5), (2, 9)
) AS v (ID_ROL, ID_MENU)
WHERE NOT EXISTS (SELECT 1 FROM PERMISO p WHERE p.ID_ROL = v.ID_ROL AND p.ID_MENU = v.ID_MENU);
GO

-- USUARIO ADMINISTRADOR INICIAL
IF NOT EXISTS (SELECT 1 FROM USUARIO WHERE CORREO_ELECTRONICO = N'admin@supermercado.com')
BEGIN
    INSERT INTO USUARIO (CODIGO, NOMBRE_COMPLETO, CORREO_ELECTRONICO, CLAVE, ID_ROL, ESTADO, ID_USUARIO_CREACION, FECHA_CREACION)
    VALUES ('00001', 'Administrador', N'admin@supermercado.com', LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'Admin123*'), 2)), 1, 1, NULL, SYSUTCDATETIME());
END;
GO
