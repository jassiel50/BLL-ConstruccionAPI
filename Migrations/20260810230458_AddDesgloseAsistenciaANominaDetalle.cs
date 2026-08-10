using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDesgloseAsistenciaANominaDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiasTrabajados",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Faltas",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasExtra",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Retardos",
                table: "NominaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SueldoDiario",
                table: "NominaDetalles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasTrabajados",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "Faltas",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "HorasExtra",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "Retardos",
                table: "NominaDetalles");

            migrationBuilder.DropColumn(
                name: "SueldoDiario",
                table: "NominaDetalles");
        }
    }
}
