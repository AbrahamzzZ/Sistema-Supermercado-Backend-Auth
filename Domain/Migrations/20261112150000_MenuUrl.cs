using Domain.Migrations.Scripts;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <summary>
    /// MENU.CODIGO_MENU ('001', '002'...) pasa a MENU.URL_MENU ('/usuario', '/compra'...), igual que en el monolito.
    /// El frontend usa la URL para el sidebar y la compara con los permisos del token (claim "permiso").
    /// </summary>
    public partial class MenuUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Renombrar (no DROP + ADD) para conservar los datos
            migrationBuilder.RenameColumn(name: "CODIGO_MENU", table: "MENU", newName: "URL_MENU");

            migrationBuilder.Sql(@"
UPDATE MENU SET URL_MENU = CASE URL_MENU
    WHEN '001' THEN '/usuario'
    WHEN '002' THEN '/compra'
    WHEN '003' THEN '/venta'
    WHEN '004' THEN '/producto'
    WHEN '005' THEN '/categoria'
    WHEN '006' THEN '/cliente'
    WHEN '007' THEN '/proveedor'
    WHEN '008' THEN '/transportista'
    WHEN '009' THEN '/oferta'
    WHEN '010' THEN '/sucursal'
    WHEN '011' THEN '/log'
    ELSE URL_MENU
END;");

            migrationBuilder.EjecutarScript("V2_01_Procedimientos_Menu.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE MENU SET URL_MENU = CASE URL_MENU
    WHEN '/usuario' THEN '001'
    WHEN '/compra' THEN '002'
    WHEN '/venta' THEN '003'
    WHEN '/producto' THEN '004'
    WHEN '/categoria' THEN '005'
    WHEN '/cliente' THEN '006'
    WHEN '/proveedor' THEN '007'
    WHEN '/transportista' THEN '008'
    WHEN '/oferta' THEN '009'
    WHEN '/sucursal' THEN '010'
    WHEN '/log' THEN '011'
    ELSE URL_MENU
END;");

            migrationBuilder.RenameColumn(name: "URL_MENU", table: "MENU", newName: "CODIGO_MENU");

            // Vuelve a la versión anterior del SP (devuelve CODIGO_MENU)
            migrationBuilder.EjecutarScript("V1_01_Procedimientos_Rol_Menu.sql");
        }
    }
}
