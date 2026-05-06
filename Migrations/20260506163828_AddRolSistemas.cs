using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRolSistemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Nombre", "Descripcion", "Activo" },
                values: new object[] { 3, "Sistemas", "Equipo de sistemas y desarrollo", true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
