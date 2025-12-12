using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class Timestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "Usuarios",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "FechaBaja",
                table: "Usuarios",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "FechaAlta",
                table: "Usuarios",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "FechaAlta",
                table: "Tutelas",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Usuarios_Salas_Roles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Salas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Salas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Jardines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Jardines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Infantes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Infantes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Usuarios_Salas_Roles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Salas");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Salas");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Jardines");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Jardines");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Infantes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Infantes");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Usuarios",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Usuarios",
                newName: "FechaBaja");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Usuarios",
                newName: "FechaAlta");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Tutelas",
                newName: "FechaAlta");
        }
    }
}
