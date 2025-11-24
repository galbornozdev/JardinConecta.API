using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class CreateTableComunicado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comunicados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Contenido = table.Column<string>(type: "text", nullable: false),
                    IdUsuarioRemitente = table.Column<Guid>(type: "uuid", nullable: false),
                    IdSala = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comunicados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comunicados_Salas_IdSala",
                        column: x => x.IdSala,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comunicados_Usuarios_IdUsuarioRemitente",
                        column: x => x.IdUsuarioRemitente,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comunicados_Usuarios_Destino",
                columns: table => new
                {
                    ComunicadosId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuariosDestinatariosId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comunicados_Usuarios_Destino", x => new { x.ComunicadosId, x.UsuariosDestinatariosId });
                    table.ForeignKey(
                        name: "FK_Comunicados_Usuarios_Destino_Comunicados_ComunicadosId",
                        column: x => x.ComunicadosId,
                        principalTable: "Comunicados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comunicados_Usuarios_Destino_Usuarios_UsuariosDestinatarios~",
                        column: x => x.UsuariosDestinatariosId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comunicados_IdSala",
                table: "Comunicados",
                column: "IdSala");

            migrationBuilder.CreateIndex(
                name: "IX_Comunicados_IdUsuarioRemitente",
                table: "Comunicados",
                column: "IdUsuarioRemitente");

            migrationBuilder.CreateIndex(
                name: "IX_Comunicados_Usuarios_Destino_UsuariosDestinatariosId",
                table: "Comunicados_Usuarios_Destino",
                column: "UsuariosDestinatariosId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comunicados_Usuarios_Destino");

            migrationBuilder.DropTable(
                name: "Comunicados");
        }
    }
}
