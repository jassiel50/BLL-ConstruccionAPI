using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddServiciosYRolOperadorServicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    ClienteNombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClienteDireccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClienteTelefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Equipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DireccionServicio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescripcionTrabajo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaterialesUtilizados = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OperadorId = table.Column<int>(type: "int", nullable: false),
                    OperadorNombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirmaBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreQuienFirma = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaFirma = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Servicios_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_ClienteId",
                table: "Servicios",
                column: "ClienteId");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Nombre", "Descripcion", "Activo" },
                values: new object[] { 4, "Operador de Servicio", "Realiza servicios en campo (instalaciones, mantenimientos) y captura la firma del cliente", true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropTable(
                name: "Servicios");
        }
    }
}
