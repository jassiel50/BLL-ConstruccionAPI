using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpleados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroEmpleado = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Puesto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CURP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RFC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NSS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactoEmergencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SueldoNetoSemanal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreditoInfonavit = table.Column<bool>(type: "bit", nullable: false),
                    TipoDescuentoInfonavit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CuotaInfonavit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosEmpleado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ArchivosEmpleado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosEmpleado_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionesEmpleadoProyecto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioAsignoId = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObservacionesFin = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesEmpleadoProyecto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionesEmpleadoProyecto_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsignacionesEmpleadoProyecto_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosEmpleado_EmpleadoId",
                table: "ArchivosEmpleado",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesEmpleadoProyecto_EmpleadoId",
                table: "AsignacionesEmpleadoProyecto",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesEmpleadoProyecto_ProyectoId",
                table: "AsignacionesEmpleadoProyecto",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_NumeroEmpleado",
                table: "Empleados",
                column: "NumeroEmpleado",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchivosEmpleado");

            migrationBuilder.DropTable(
                name: "AsignacionesEmpleadoProyecto");

            migrationBuilder.DropTable(
                name: "Empleados");
        }
    }
}
