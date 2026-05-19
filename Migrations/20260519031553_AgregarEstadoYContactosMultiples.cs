using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadoYContactosMultiples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Crear nuevas tablas primero
            migrationBuilder.CreateTable(
                name: "ContactosCliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactosCliente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactosCliente_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactosProveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactosProveedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactosProveedor_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactosCliente_ClienteId",
                table: "ContactosCliente",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactosProveedor_ProveedorId",
                table: "ContactosProveedor",
                column: "ProveedorId");

            // 2. Migrar datos existentes de contacto único a las nuevas tablas
            migrationBuilder.Sql(@"
                INSERT INTO ContactosCliente (ClienteId, Nombre, Telefono, Email, Cargo, EsPrincipal)
                SELECT Id,
                       CASE WHEN Contacto != '' THEN Contacto ELSE 'Sin nombre' END,
                       ISNULL(Telefono, ''),
                       ISNULL(Email, ''),
                       '',
                       1
                FROM Clientes
                WHERE Contacto != '' OR Telefono != '' OR Email != ''
            ");

            migrationBuilder.Sql(@"
                INSERT INTO ContactosProveedor (ProveedorId, Nombre, Telefono, Email, Cargo, EsPrincipal)
                SELECT Id,
                       CASE WHEN Contacto != '' THEN Contacto ELSE 'Sin nombre' END,
                       ISNULL(Telefono, ''),
                       ISNULL(Email, ''),
                       '',
                       1
                FROM Proveedores
                WHERE Contacto != '' OR Telefono != '' OR Email != ''
            ");

            // 3. Agregar columna Estado
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Proveedores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // 4. Borrar columnas viejas (datos ya migrados)
            migrationBuilder.DropColumn(name: "Contacto", table: "Proveedores");
            migrationBuilder.DropColumn(name: "Email",    table: "Proveedores");
            migrationBuilder.DropColumn(name: "Telefono", table: "Proveedores");
            migrationBuilder.DropColumn(name: "Contacto", table: "Clientes");
            migrationBuilder.DropColumn(name: "Email",    table: "Clientes");
            migrationBuilder.DropColumn(name: "Telefono", table: "Clientes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactosCliente");

            migrationBuilder.DropTable(
                name: "ContactosProveedor");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Clientes");

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Contacto",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Contacto",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
