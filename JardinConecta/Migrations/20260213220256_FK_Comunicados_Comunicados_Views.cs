using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class FK_Comunicados_Comunicados_Views : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ComunicadosViews_IdUsuario",
                table: "ComunicadosViews",
                column: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_ComunicadosViews_Comunicados_IdUsuario",
                table: "ComunicadosViews",
                column: "IdUsuario",
                principalTable: "Comunicados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComunicadosViews_Comunicados_IdUsuario",
                table: "ComunicadosViews");

            migrationBuilder.DropIndex(
                name: "IX_ComunicadosViews_IdUsuario",
                table: "ComunicadosViews");
        }
    }
}
