using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCarpetasProyecto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CarpetaId",
                table: "ArchivosProyecto",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CarpetasProyecto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreadoPorId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarpetasProyecto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarpetasProyecto_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosProyecto_CarpetaId",
                table: "ArchivosProyecto",
                column: "CarpetaId");

            migrationBuilder.CreateIndex(
                name: "IX_CarpetasProyecto_ProyectoId_Nombre",
                table: "CarpetasProyecto",
                columns: new[] { "ProyectoId", "Nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ArchivosProyecto_CarpetasProyecto_CarpetaId",
                table: "ArchivosProyecto",
                column: "CarpetaId",
                principalTable: "CarpetasProyecto",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchivosProyecto_CarpetasProyecto_CarpetaId",
                table: "ArchivosProyecto");

            migrationBuilder.DropTable(
                name: "CarpetasProyecto");

            migrationBuilder.DropIndex(
                name: "IX_ArchivosProyecto_CarpetaId",
                table: "ArchivosProyecto");

            migrationBuilder.DropColumn(
                name: "CarpetaId",
                table: "ArchivosProyecto");
        }
    }
}
