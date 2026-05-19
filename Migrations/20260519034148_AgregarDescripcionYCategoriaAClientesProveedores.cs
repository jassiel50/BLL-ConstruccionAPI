using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDescripcionYCategoriaAClientesProveedores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoriaId",
                table: "Proveedores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CategoriaId",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CategoriasCliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasCliente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasProveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasProveedor", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_CategoriaId",
                table: "Proveedores",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_CategoriaId",
                table: "Clientes",
                column: "CategoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_CategoriasCliente_CategoriaId",
                table: "Clientes",
                column: "CategoriaId",
                principalTable: "CategoriasCliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Proveedores_CategoriasProveedor_CategoriaId",
                table: "Proveedores",
                column: "CategoriaId",
                principalTable: "CategoriasProveedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_CategoriasCliente_CategoriaId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Proveedores_CategoriasProveedor_CategoriaId",
                table: "Proveedores");

            migrationBuilder.DropTable(
                name: "CategoriasCliente");

            migrationBuilder.DropTable(
                name: "CategoriasProveedor");

            migrationBuilder.DropIndex(
                name: "IX_Proveedores_CategoriaId",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_CategoriaId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Clientes");
        }
    }
}
