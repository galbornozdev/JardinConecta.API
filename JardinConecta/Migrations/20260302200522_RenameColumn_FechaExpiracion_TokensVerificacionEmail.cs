using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumn_FechaExpiracion_TokensVerificacionEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Expiracion",
                table: "TokensVerificacionEmail",
                newName: "FechaExpiracion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaExpiracion",
                table: "TokensVerificacionEmail",
                newName: "Expiracion");
        }
    }
}
