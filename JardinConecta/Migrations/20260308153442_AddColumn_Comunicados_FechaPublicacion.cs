using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class AddColumn_Comunicados_FechaPublicacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPublicacion",
                table: "Comunicados",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaPublicacion",
                table: "Comunicados");
        }
    }
}
