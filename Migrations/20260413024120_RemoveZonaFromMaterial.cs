using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveZonaFromMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoUbicacion",
                table: "Materiales");

            migrationBuilder.DropColumn(
                name: "Zona",
                table: "Materiales");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
