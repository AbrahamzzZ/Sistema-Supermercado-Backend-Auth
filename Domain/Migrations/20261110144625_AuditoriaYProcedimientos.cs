using System;
using Domain.Migrations.Scripts;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AuditoriaYProcedimientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FECHA_CREACION pasa a NOT NULL: se rellenan los posibles nulos antes de cambiar la columna
            migrationBuilder.Sql("UPDATE USUARIO SET FECHA_CREACION = SYSUTCDATETIME() WHERE FECHA_CREACION IS NULL;");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "USUARIO",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<bool>(
                name: "ELIMINADO",
                table: "USUARIO",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_MODIFICACION",
                table: "USUARIO",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_CREACION",
                table: "USUARIO",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_MODIFICACION",
                table: "USUARIO",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "ROL",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "MENU",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            // Procedimientos almacenados y datos iniciales (archivos .sql embebidos en Migrations/Scripts)
            migrationBuilder.EjecutarScript("V1_01_Procedimientos_Rol_Menu.sql");
            migrationBuilder.EjecutarScript("V1_02_Procedimientos_Usuario.sql");
            migrationBuilder.EjecutarScript("V1_03_Datos_Iniciales.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Los datos iniciales se conservan; se eliminan los procedimientos creados en Up()
            foreach (var sp in new[]
            {
                "PA_LISTA_ROL", "PA_OBTENER_MENU",
                "PA_LISTA_USUARIO", "PA_LISTA_USUARIO_PAGINACION", "PA_OBTENER_USUARIO", "PA_INICIAR_SESION",
                "PA_ACTUALIZAR_CLAVE", "PA_REGISTRAR_USUARIO", "PA_EDITAR_USUARIO", "PA_ELIMINAR_USUARIO"
            })
            {
                migrationBuilder.Sql($"DROP PROCEDURE IF EXISTS {sp};");
            }

            migrationBuilder.DropColumn(
                name: "ELIMINADO",
                table: "USUARIO");

            migrationBuilder.DropColumn(
                name: "FECHA_MODIFICACION",
                table: "USUARIO");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_CREACION",
                table: "USUARIO");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_MODIFICACION",
                table: "USUARIO");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "USUARIO",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "ROL",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "MENU",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "(sysutcdatetime())");
        }
    }
}
