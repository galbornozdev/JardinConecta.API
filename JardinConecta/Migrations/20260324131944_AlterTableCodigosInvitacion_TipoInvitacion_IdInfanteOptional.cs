using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class AlterTableCodigosInvitacion_TipoInvitacion_IdInfanteOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodigosInvitacion_Infantes_IdInfante",
                table: "CodigosInvitacion");

            migrationBuilder.AlterColumn<Guid>(
                name: "IdInfante",
                table: "CodigosInvitacion",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "TipoInvitacion",
                table: "CodigosInvitacion",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_CodigosInvitacion_Infantes_IdInfante",
                table: "CodigosInvitacion",
                column: "IdInfante",
                principalTable: "Infantes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodigosInvitacion_Infantes_IdInfante",
                table: "CodigosInvitacion");

            migrationBuilder.DropColumn(
                name: "TipoInvitacion",
                table: "CodigosInvitacion");

            migrationBuilder.AlterColumn<Guid>(
                name: "IdInfante",
                table: "CodigosInvitacion",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CodigosInvitacion_Infantes_IdInfante",
                table: "CodigosInvitacion",
                column: "IdInfante",
                principalTable: "Infantes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
