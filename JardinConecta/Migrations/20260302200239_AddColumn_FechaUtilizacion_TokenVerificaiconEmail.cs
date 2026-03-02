using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class AddColumn_FechaUtilizacion_TokenVerificaiconEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUtilizacion",
                table: "TokensVerificacionEmail",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaUtilizacion",
                table: "TokensVerificacionEmail");
        }
    }
}
