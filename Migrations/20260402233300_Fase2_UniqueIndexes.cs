using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class Fase2_UniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AlmacenProyecto_ProyectoId",
                table: "AlmacenProyecto");

            migrationBuilder.DropIndex(
                name: "IX_AlmacenCentral_MaterialId",
                table: "AlmacenCentral");

            migrationBuilder.AlterColumn<string>(
                name: "NombreUsuario",
                table: "Usuarios",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Abreviatura",
                table: "UnidadesMedida",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "TokensSesion",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RFC",
                table: "Proveedores",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Materiales",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroSerie",
                table: "Herramientas",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Herramientas",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RFC",
                table: "Clientes",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NombreUsuario",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesMedida_Abreviatura",
                table: "UnidadesMedida",
                column: "Abreviatura",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokensSesion_Token",
                table: "TokensSesion",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_RFC",
                table: "Proveedores",
                column: "RFC",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materiales_Codigo",
                table: "Materiales",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Herramientas_Codigo",
                table: "Herramientas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Herramientas_NumeroSerie",
                table: "Herramientas",
                column: "NumeroSerie",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_RFC",
                table: "Clientes",
                column: "RFC",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlmacenProyecto_ProyectoId_MaterialId",
                table: "AlmacenProyecto",
                columns: new[] { "ProyectoId", "MaterialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlmacenCentral_MaterialId",
                table: "AlmacenCentral",
                column: "MaterialId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_NombreUsuario",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_UnidadesMedida_Abreviatura",
                table: "UnidadesMedida");

            migrationBuilder.DropIndex(
                name: "IX_TokensSesion_Token",
                table: "TokensSesion");

            migrationBuilder.DropIndex(
                name: "IX_Proveedores_RFC",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Materiales_Codigo",
                table: "Materiales");

            migrationBuilder.DropIndex(
                name: "IX_Herramientas_Codigo",
                table: "Herramientas");

            migrationBuilder.DropIndex(
                name: "IX_Herramientas_NumeroSerie",
                table: "Herramientas");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_RFC",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_AlmacenProyecto_ProyectoId_MaterialId",
                table: "AlmacenProyecto");

            migrationBuilder.DropIndex(
                name: "IX_AlmacenCentral_MaterialId",
                table: "AlmacenCentral");

            migrationBuilder.AlterColumn<string>(
                name: "NombreUsuario",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Abreviatura",
                table: "UnidadesMedida",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "TokensSesion",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "RFC",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Materiales",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroSerie",
                table: "Herramientas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Herramientas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "RFC",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_AlmacenProyecto_ProyectoId",
                table: "AlmacenProyecto",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_AlmacenCentral_MaterialId",
                table: "AlmacenCentral",
                column: "MaterialId");
        }
    }
}
