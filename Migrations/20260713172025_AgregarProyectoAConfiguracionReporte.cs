using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProyectoAConfiguracionReporte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProyectoId",
                table: "ConfiguracionesReporte",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesReporte_ProyectoId",
                table: "ConfiguracionesReporte",
                column: "ProyectoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesReporte_Proyectos_ProyectoId",
                table: "ConfiguracionesReporte",
                column: "ProyectoId",
                principalTable: "Proyectos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesReporte_Proyectos_ProyectoId",
                table: "ConfiguracionesReporte");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesReporte_ProyectoId",
                table: "ConfiguracionesReporte");

            migrationBuilder.DropColumn(
                name: "ProyectoId",
                table: "ConfiguracionesReporte");
        }
    }
}
