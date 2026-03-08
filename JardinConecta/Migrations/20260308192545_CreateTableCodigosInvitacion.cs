using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class CreateTableCodigosInvitacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodigosInvitacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    IdSala = table.Column<Guid>(type: "uuid", nullable: false),
                    IdInfante = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigosInvitacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodigosInvitacion_Infantes_IdInfante",
                        column: x => x.IdInfante,
                        principalTable: "Infantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CodigosInvitacion_Salas_IdSala",
                        column: x => x.IdSala,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodigosInvitacion_Codigo",
                table: "CodigosInvitacion",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodigosInvitacion_IdInfante",
                table: "CodigosInvitacion",
                column: "IdInfante");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosInvitacion_IdSala",
                table: "CodigosInvitacion",
                column: "IdSala");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodigosInvitacion");
        }
    }
}
