using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class AddTableCodigoInvitacion_Infantes_RemoveColumnIdInfante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodigosInvitacion_Infantes_IdInfante",
                table: "CodigosInvitacion");

            migrationBuilder.DropIndex(
                name: "IX_CodigosInvitacion_IdInfante",
                table: "CodigosInvitacion");

            migrationBuilder.DropColumn(
                name: "IdInfante",
                table: "CodigosInvitacion");

            migrationBuilder.CreateTable(
                name: "CodigoInvitacion_Infantes",
                columns: table => new
                {
                    IdCodigoInvitacion = table.Column<Guid>(type: "uuid", nullable: false),
                    IdInfante = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigoInvitacion_Infantes", x => new { x.IdCodigoInvitacion, x.IdInfante });
                    table.ForeignKey(
                        name: "FK_CodigoInvitacion_Infantes_CodigosInvitacion_IdCodigoInvitac~",
                        column: x => x.IdCodigoInvitacion,
                        principalTable: "CodigosInvitacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CodigoInvitacion_Infantes_Infantes_IdInfante",
                        column: x => x.IdInfante,
                        principalTable: "Infantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodigoInvitacion_Infantes_IdInfante",
                table: "CodigoInvitacion_Infantes",
                column: "IdInfante");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodigoInvitacion_Infantes");

            migrationBuilder.AddColumn<Guid>(
                name: "IdInfante",
                table: "CodigosInvitacion",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodigosInvitacion_IdInfante",
                table: "CodigosInvitacion",
                column: "IdInfante");

            migrationBuilder.AddForeignKey(
                name: "FK_CodigosInvitacion_Infantes_IdInfante",
                table: "CodigosInvitacion",
                column: "IdInfante",
                principalTable: "Infantes",
                principalColumn: "Id");
        }
    }
}
