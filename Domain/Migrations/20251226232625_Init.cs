using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MENU",
                columns: table => new
                {
                    ID_MENU = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOMBRE_MENU = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CODIGO_MENU = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    NOMBRE_ICONO = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    FECHA_CREACION = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MENU", x => x.ID_MENU);
                });

            migrationBuilder.CreateTable(
                name: "ROL",
                columns: table => new
                {
                    ID_ROL = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOMBRE = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    FECHA_CREACION = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROL", x => x.ID_ROL);
                });

            migrationBuilder.CreateTable(
                name: "PERMISO",
                columns: table => new
                {
                    ID_PERMISO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ROL = table.Column<int>(type: "int", nullable: true),
                    ID_MENU = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PERMISO", x => x.ID_PERMISO);
                    table.ForeignKey(
                        name: "FK_PERMISO_MENU_ID_MENU",
                        column: x => x.ID_MENU,
                        principalTable: "MENU",
                        principalColumn: "ID_MENU",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PERMISO_ROL_ID_ROL",
                        column: x => x.ID_ROL,
                        principalTable: "ROL",
                        principalColumn: "ID_ROL",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    ID_USUARIO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CODIGO = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    NOMBRE_COMPLETO = table.Column<string>(type: "varchar(70)", unicode: false, maxLength: 70, nullable: true),
                    CORREO_ELECTRONICO = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CLAVE = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ID_ROL = table.Column<int>(type: "int", nullable: true),
                    ESTADO = table.Column<bool>(type: "bit", nullable: true),
                    FECHA_CREACION = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIO", x => x.ID_USUARIO);
                    table.ForeignKey(
                        name: "FK_USUARIO_ROL_ID_ROL",
                        column: x => x.ID_ROL,
                        principalTable: "ROL",
                        principalColumn: "ID_ROL",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PERMISO_ID_MENU",
                table: "PERMISO",
                column: "ID_MENU");

            migrationBuilder.CreateIndex(
                name: "IX_PERMISO_ID_ROL",
                table: "PERMISO",
                column: "ID_ROL");

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_ID_ROL",
                table: "USUARIO",
                column: "ID_ROL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PERMISO");

            migrationBuilder.DropTable(
                name: "USUARIO");

            migrationBuilder.DropTable(
                name: "MENU");

            migrationBuilder.DropTable(
                name: "ROL");
        }
    }
}
