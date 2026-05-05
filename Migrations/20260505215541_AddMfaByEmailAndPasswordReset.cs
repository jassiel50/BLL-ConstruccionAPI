using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMfaByEmailAndPasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MfaEmailCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    CodigoHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Canal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaExpira = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IntentosFallidos = table.Column<int>(type: "int", nullable: false),
                    MaxIntentos = table.Column<int>(type: "int", nullable: false),
                    Usado = table.Column<bool>(type: "bit", nullable: false),
                    IpSolicitud = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpVerificacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MfaEmailCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MfaEmailCodes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    CodigoHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaExpira = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IntentosFallidos = table.Column<int>(type: "int", nullable: false),
                    MaxIntentos = table.Column<int>(type: "int", nullable: false),
                    Usado = table.Column<bool>(type: "bit", nullable: false),
                    IpSolicitud = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpVerificacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetCodes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosMfaConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    MfaHabilitado = table.Column<bool>(type: "bit", nullable: false),
                    MfaMetodoPreferido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MfaEmailHabilitado = table.Column<bool>(type: "bit", nullable: false),
                    MfaAppHabilitado = table.Column<bool>(type: "bit", nullable: false),
                    MfaUltimaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosMfaConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosMfaConfig_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MfaEmailCodes_FechaExpira",
                table: "MfaEmailCodes",
                column: "FechaExpira");

            migrationBuilder.CreateIndex(
                name: "IX_MfaEmailCodes_UsuarioId_Usado_FechaExpira",
                table: "MfaEmailCodes",
                columns: new[] { "UsuarioId", "Usado", "FechaExpira" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetCodes_FechaExpira",
                table: "PasswordResetCodes",
                column: "FechaExpira");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetCodes_UsuarioId_Usado_FechaExpira",
                table: "PasswordResetCodes",
                columns: new[] { "UsuarioId", "Usado", "FechaExpira" });

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosMfaConfig_UsuarioId",
                table: "UsuariosMfaConfig",
                column: "UsuarioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MfaEmailCodes");

            migrationBuilder.DropTable(
                name: "PasswordResetCodes");

            migrationBuilder.DropTable(
                name: "UsuariosMfaConfig");
        }
    }
}
