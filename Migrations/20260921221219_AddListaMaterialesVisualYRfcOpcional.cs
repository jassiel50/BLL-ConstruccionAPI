using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddListaMaterialesVisualYRfcOpcional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_RFC",
                table: "Clientes");

            migrationBuilder.AddColumn<string>(
                name: "ListaMaterialesVisual",
                table: "Proyectos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_RFC",
                table: "Clientes",
                column: "RFC",
                unique: true,
                filter: "[RFC] <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_RFC",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "ListaMaterialesVisual",
                table: "Proyectos");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_RFC",
                table: "Clientes",
                column: "RFC",
                unique: true);
        }
    }
}
