using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRequisicionMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequisicionesMaterial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    Folio = table.Column<int>(type: "int", nullable: false),
                    SeRequierePara = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeSuministraPor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SolicitoNombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreadoPorId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisicionesMaterial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequisicionesMaterial_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequisicionesMaterialDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequisicionMaterialId = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AreaComentarios = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisicionesMaterialDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequisicionesMaterialDetalle_Materiales_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materiales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RequisicionesMaterialDetalle_RequisicionesMaterial_RequisicionMaterialId",
                        column: x => x.RequisicionMaterialId,
                        principalTable: "RequisicionesMaterial",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesMaterial_ProyectoId",
                table: "RequisicionesMaterial",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesMaterialDetalle_MaterialId",
                table: "RequisicionesMaterialDetalle",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesMaterialDetalle_RequisicionMaterialId",
                table: "RequisicionesMaterialDetalle",
                column: "RequisicionMaterialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequisicionesMaterialDetalle");

            migrationBuilder.DropTable(
                name: "RequisicionesMaterial");
        }
    }
}
