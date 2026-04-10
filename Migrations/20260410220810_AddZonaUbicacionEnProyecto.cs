using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddZonaUbicacionEnProyecto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnProyecto",
                table: "Materiales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TipoUbicacion",
                table: "Materiales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Zona",
                table: "Materiales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TipoUbicacion",
                table: "Herramientas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Zona",
                table: "Herramientas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TipoUbicacion",
                table: "AlmacenCentral",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Zona",
                table: "AlmacenCentral",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnProyecto",
                table: "Materiales");

            migrationBuilder.DropColumn(
                name: "TipoUbicacion",
                table: "Materiales");

            migrationBuilder.DropColumn(
                name: "Zona",
                table: "Materiales");

            migrationBuilder.DropColumn(
                name: "TipoUbicacion",
                table: "Herramientas");

            migrationBuilder.DropColumn(
                name: "Zona",
                table: "Herramientas");

            migrationBuilder.DropColumn(
                name: "TipoUbicacion",
                table: "AlmacenCentral");

            migrationBuilder.DropColumn(
                name: "Zona",
                table: "AlmacenCentral");
        }
    }
}
