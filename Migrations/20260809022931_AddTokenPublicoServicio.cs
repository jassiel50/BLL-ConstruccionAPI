using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenPublicoServicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpira",
                table: "Servicios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenPublico",
                table: "Servicios",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_TokenPublico",
                table: "Servicios",
                column: "TokenPublico",
                unique: true,
                filter: "[TokenPublico] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Servicios_TokenPublico",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "TokenExpira",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "TokenPublico",
                table: "Servicios");
        }
    }
}
