using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JardinConecta.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_ScheduledTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduledTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Cron = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Habilitado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTasks", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ScheduledTasks",
                columns: new[] { "Id", "Cron", "Descripcion", "Habilitado", "Nombre" },
                values: new object[] { 1, "*/15 * * * *", "Publica los comunicados programados cuya fecha de envío ya fue alcanzada", true, "ComunicadosProgramados" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledTasks");
        }
    }
}
