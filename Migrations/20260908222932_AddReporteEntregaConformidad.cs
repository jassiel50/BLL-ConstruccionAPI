using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddReporteEntregaConformidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportesEntregaConformidad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Folio = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProyectoId = table.Column<int>(type: "int", nullable: true),
                    ProyectoNombreLibre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    EmpresaNombreLibre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactoNombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Concepto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrdenCompra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CondicionesEntrega = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntregaNombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntregaEmpresa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecibeNombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecibeEmpresa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentoFirmadoNombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentoFirmadoContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentoFirmadoContenido = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    DocumentoFirmadoFecha = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPorId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesEntregaConformidad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportesEntregaConformidad_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReportesEntregaConformidad_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReportesEntregaConformidadFotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReporteEntregaConformidadId = table.Column<int>(type: "int", nullable: false),
                    NombreOriginal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    Contenido = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FechaCaptura = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesEntregaConformidadFotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportesEntregaConformidadFotos_ReportesEntregaConformidad_ReporteEntregaConformidadId",
                        column: x => x.ReporteEntregaConformidadId,
                        principalTable: "ReportesEntregaConformidad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportesEntregaConformidad_ClienteId",
                table: "ReportesEntregaConformidad",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesEntregaConformidad_ProyectoId",
                table: "ReportesEntregaConformidad",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesEntregaConformidadFotos_ReporteEntregaConformidadId",
                table: "ReportesEntregaConformidadFotos",
                column: "ReporteEntregaConformidadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportesEntregaConformidadFotos");

            migrationBuilder.DropTable(
                name: "ReportesEntregaConformidad");
        }
    }
}
