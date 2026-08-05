using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddServicioSolicitanteFotosYRecursos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFirmaSolicitante",
                table: "Servicios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirmaSolicitanteBase64",
                table: "Servicios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HorarioTrabajo",
                table: "Servicios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreSolicitante",
                table: "Servicios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroTrabajadores",
                table: "Servicios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecursoConsumibles",
                table: "Servicios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecursoHerramienta",
                table: "Servicios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecursoManoDeObra",
                table: "Servicios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecursoRefacciones",
                table: "Servicios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TiposTrabajo",
                table: "Servicios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalHorasTrabajadas",
                table: "Servicios",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServiciosFotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    NombreOriginal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    Contenido = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FechaCaptura = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiciosFotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiciosFotos_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosFotos_ServicioId",
                table: "ServiciosFotos",
                column: "ServicioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiciosFotos");

            migrationBuilder.DropColumn(
                name: "FechaFirmaSolicitante",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "FirmaSolicitanteBase64",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "HorarioTrabajo",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "NombreSolicitante",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "NumeroTrabajadores",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "RecursoConsumibles",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "RecursoHerramienta",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "RecursoManoDeObra",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "RecursoRefacciones",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "TiposTrabajo",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "TotalHorasTrabajadas",
                table: "Servicios");
        }
    }
}
