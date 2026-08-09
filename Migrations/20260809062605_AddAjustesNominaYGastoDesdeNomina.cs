using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAjustesNominaYGastoDesdeNomina : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MontoAjuste",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAjuste",
                table: "NominaDetalles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PeriodoNominaId",
                table: "GastosSemanales",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GastosSemanales_PeriodoNominaId",
                table: "GastosSemanales",
                column: "PeriodoNominaId");

            migrationBuilder.AddForeignKey(
                name: "FK_GastosSemanales_PeriodosNomina_PeriodoNominaId",
                table: "GastosSemanales",
                column: "PeriodoNominaId",
                principalTable: "PeriodosNomina",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GastosSemanales_PeriodosNomina_PeriodoNominaId",
                table: "GastosSemanales");

            migrationBuilder.DropIndex(
                name: "IX_GastosSemanales_PeriodoNominaId",
                table: "GastosSemanales");

            migrationBuilder.DropColumn(
                name: "MontoAjuste",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "MotivoAjuste",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "PeriodoNominaId",
                table: "GastosSemanales");
        }
    }
}
