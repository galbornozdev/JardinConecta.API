using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class AddColumn_Estado_FechaPrograma_Comunicados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Comunicados",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPrograma",
                table: "Comunicados",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Comunicados");

            migrationBuilder.DropColumn(
                name: "FechaPrograma",
                table: "Comunicados");
        }
    }
}
