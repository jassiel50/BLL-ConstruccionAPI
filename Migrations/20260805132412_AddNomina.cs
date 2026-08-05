using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNomina : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeriodosNomina",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GeneradoPorId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodosNomina", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NominaDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodoNominaId = table.Column<int>(type: "int", nullable: false),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    ProyectoId = table.Column<int>(type: "int", nullable: true),
                    SueldoBruto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DescuentoInfonavit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SueldoNeto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Pagado = table.Column<bool>(type: "bit", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NominaDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NominaDetalles_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NominaDetalles_PeriodosNomina_PeriodoNominaId",
                        column: x => x.PeriodoNominaId,
                        principalTable: "PeriodosNomina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NominaDetalles_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NominaDetalles_EmpleadoId",
                table: "NominaDetalles",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_NominaDetalles_PeriodoNominaId",
                table: "NominaDetalles",
                column: "PeriodoNominaId");

            migrationBuilder.CreateIndex(
                name: "IX_NominaDetalles_ProyectoId",
                table: "NominaDetalles",
                column: "ProyectoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NominaDetalles");

            migrationBuilder.DropTable(
                name: "PeriodosNomina");
        }
    }
}
