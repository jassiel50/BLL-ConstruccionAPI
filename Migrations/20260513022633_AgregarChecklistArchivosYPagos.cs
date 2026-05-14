using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarChecklistArchivosYPagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CobradoCliente",
                table: "GastosExtras",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoProveedor",
                table: "GastosExtras",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ProveedorId",
                table: "GastosExtras",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ArchivosProyecto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    NombreOriginal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    Contenido = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    SubidoPorId = table.Column<int>(type: "int", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosProyecto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosProyecto_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    FaseId = table.Column<int>(type: "int", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Completado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCompletado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletadoPorId = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistItems_FaseProyectos_FaseId",
                        column: x => x.FaseId,
                        principalTable: "FaseProyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChecklistItems_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PagosCliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    Concepto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistradoPorId = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosCliente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosCliente_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GastosExtras_ProveedorId",
                table: "GastosExtras",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosProyecto_ProyectoId",
                table: "ArchivosProyecto",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItems_FaseId",
                table: "ChecklistItems",
                column: "FaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItems_ProyectoId",
                table: "ChecklistItems",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosCliente_ProyectoId",
                table: "PagosCliente",
                column: "ProyectoId");

            migrationBuilder.AddForeignKey(
                name: "FK_GastosExtras_Proveedores_ProveedorId",
                table: "GastosExtras",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GastosExtras_Proveedores_ProveedorId",
                table: "GastosExtras");

            migrationBuilder.DropTable(
                name: "ArchivosProyecto");

            migrationBuilder.DropTable(
                name: "ChecklistItems");

            migrationBuilder.DropTable(
                name: "PagosCliente");

            migrationBuilder.DropIndex(
                name: "IX_GastosExtras_ProveedorId",
                table: "GastosExtras");

            migrationBuilder.DropColumn(
                name: "CobradoCliente",
                table: "GastosExtras");

            migrationBuilder.DropColumn(
                name: "MontoProveedor",
                table: "GastosExtras");

            migrationBuilder.DropColumn(
                name: "ProveedorId",
                table: "GastosExtras");
        }
    }
}
