using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_TokenVerificacionEmail_AddColumn_FechaVerificacionEmail_Usuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVerificacionEmail",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TokensVerificacionEmail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Expiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokensVerificacionEmail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokensVerificacionEmail_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokensVerificacionEmail_IdUsuario",
                table: "TokensVerificacionEmail",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_TokensVerificacionEmail_Token",
                table: "TokensVerificacionEmail",
                column: "Token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokensVerificacionEmail");

            migrationBuilder.DropColumn(
                name: "FechaVerificacionEmail",
                table: "Usuarios");
        }
    }
}
