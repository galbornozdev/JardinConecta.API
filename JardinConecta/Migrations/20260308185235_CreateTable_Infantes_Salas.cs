using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_Infantes_Salas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Infantes_Salas",
                columns: table => new
                {
                    IdInfante = table.Column<Guid>(type: "uuid", nullable: false),
                    IdSala = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Infantes_Salas", x => new { x.IdInfante, x.IdSala });
                    table.ForeignKey(
                        name: "FK_Infantes_Salas_Infantes_IdInfante",
                        column: x => x.IdInfante,
                        principalTable: "Infantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Infantes_Salas_Salas_IdSala",
                        column: x => x.IdSala,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Infantes_Salas_IdSala",
                table: "Infantes_Salas",
                column: "IdSala");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Infantes_Salas");
        }
    }
}
