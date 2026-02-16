using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class FKComunicadosView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComunicadosViews_Comunicados_IdUsuario",
                table: "ComunicadosViews");

            migrationBuilder.AddForeignKey(
                name: "FK_ComunicadosViews_Comunicados_IdComunicado",
                table: "ComunicadosViews",
                column: "IdComunicado",
                principalTable: "Comunicados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComunicadosViews_Usuarios_IdUsuario",
                table: "ComunicadosViews",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComunicadosViews_Comunicados_IdComunicado",
                table: "ComunicadosViews");

            migrationBuilder.DropForeignKey(
                name: "FK_ComunicadosViews_Usuarios_IdUsuario",
                table: "ComunicadosViews");

            migrationBuilder.AddForeignKey(
                name: "FK_ComunicadosViews_Comunicados_IdUsuario",
                table: "ComunicadosViews",
                column: "IdUsuario",
                principalTable: "Comunicados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
