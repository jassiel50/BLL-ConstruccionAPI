using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancieroProyecto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Presupuesto",
                table: "Proyectos",
                newName: "PresupuestoEstimado");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnitario",
                table: "SalidasDetalle",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoContrato",
                table: "Proyectos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "GastosExtras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaseId = table.Column<int>(type: "int", nullable: false),
                    Concepto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GastosExtras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GastosExtras_FaseProyectos_FaseId",
                        column: x => x.FaseId,
                        principalTable: "FaseProyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GastosExtras_FaseId",
                table: "GastosExtras",
                column: "FaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GastosExtras");

            migrationBuilder.DropColumn(
                name: "PrecioUnitario",
                table: "SalidasDetalle");

            migrationBuilder.DropColumn(
                name: "MontoContrato",
                table: "Proyectos");

            migrationBuilder.RenameColumn(
                name: "PresupuestoEstimado",
                table: "Proyectos",
                newName: "Presupuesto");
        }
    }
}
