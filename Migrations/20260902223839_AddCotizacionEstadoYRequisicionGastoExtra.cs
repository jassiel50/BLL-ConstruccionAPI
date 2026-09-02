using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCotizacionEstadoYRequisicionGastoExtra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostoUnitario",
                table: "RequisicionesMaterialDetalle",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "GastoExtraId",
                table: "RequisicionesMaterialDetalle",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Responsable",
                table: "RequisicionesMaterialDetalle",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FaseId",
                table: "RequisicionesMaterial",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Generada");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesMaterialDetalle_GastoExtraId",
                table: "RequisicionesMaterialDetalle",
                column: "GastoExtraId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesMaterial_FaseId",
                table: "RequisicionesMaterial",
                column: "FaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequisicionesMaterial_FaseProyectos_FaseId",
                table: "RequisicionesMaterial",
                column: "FaseId",
                principalTable: "FaseProyectos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RequisicionesMaterialDetalle_GastosExtras_GastoExtraId",
                table: "RequisicionesMaterialDetalle",
                column: "GastoExtraId",
                principalTable: "GastosExtras",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequisicionesMaterial_FaseProyectos_FaseId",
                table: "RequisicionesMaterial");

            migrationBuilder.DropForeignKey(
                name: "FK_RequisicionesMaterialDetalle_GastosExtras_GastoExtraId",
                table: "RequisicionesMaterialDetalle");

            migrationBuilder.DropIndex(
                name: "IX_RequisicionesMaterialDetalle_GastoExtraId",
                table: "RequisicionesMaterialDetalle");

            migrationBuilder.DropIndex(
                name: "IX_RequisicionesMaterial_FaseId",
                table: "RequisicionesMaterial");

            migrationBuilder.DropColumn(
                name: "CostoUnitario",
                table: "RequisicionesMaterialDetalle");

            migrationBuilder.DropColumn(
                name: "GastoExtraId",
                table: "RequisicionesMaterialDetalle");

            migrationBuilder.DropColumn(
                name: "Responsable",
                table: "RequisicionesMaterialDetalle");

            migrationBuilder.DropColumn(
                name: "FaseId",
                table: "RequisicionesMaterial");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Cotizaciones");
        }
    }
}
