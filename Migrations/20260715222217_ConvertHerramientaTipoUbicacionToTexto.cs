using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class ConvertHerramientaTipoUbicacionToTexto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TipoUbicacion",
                table: "Herramientas",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // SQL Server convierte el int previo a su representación literal ("0"/"1").
            // Se traduce a los nombres del enum original para no perder la información existente.
            migrationBuilder.Sql(
                "UPDATE Herramientas SET TipoUbicacion = CASE TipoUbicacion WHEN '0' THEN 'Oficina' WHEN '1' THEN 'Almacen' ELSE TipoUbicacion END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Solo revierte de forma exacta si el valor sigue siendo "Oficina"/"Almacen".
            // Cualquier texto libre capturado después de esta migración se mapea a 1 (Almacen) por defecto.
            migrationBuilder.Sql(
                "UPDATE Herramientas SET TipoUbicacion = CASE WHEN TipoUbicacion = 'Oficina' THEN '0' WHEN TipoUbicacion = 'Almacen' THEN '1' ELSE '1' END;");

            migrationBuilder.AlterColumn<int>(
                name: "TipoUbicacion",
                table: "Herramientas",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);
        }
    }
}
