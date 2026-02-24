using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_ComunicadoArchivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComunicadosArchivos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdComunicado = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreArchivoOriginal = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Extension = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComunicadosArchivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComunicadosArchivos_Comunicados_IdComunicado",
                        column: x => x.IdComunicado,
                        principalTable: "Comunicados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Descripcion",
                value: "Familia");

            migrationBuilder.CreateIndex(
                name: "IX_ComunicadosArchivos_IdComunicado",
                table: "ComunicadosArchivos",
                column: "IdComunicado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComunicadosArchivos");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Descripcion",
                value: "Tutor");
        }
    }
}
