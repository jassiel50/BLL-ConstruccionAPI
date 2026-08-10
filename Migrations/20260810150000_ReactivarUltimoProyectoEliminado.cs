using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class ReactivarUltimoProyectoEliminado : Migration
    {
        /// <summary>
        /// Corrección de datos: un proyecto fue eliminado por error (borrado lógico, Activo = 0)
        /// justo después de crearse. Reactiva el proyecto inactivo más reciente (mayor Id).
        /// Es idempotente: si ya no hay proyectos inactivos, no hace nada.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE Proyectos
SET Activo = 1
WHERE Id = (SELECT MAX(Id) FROM Proyectos WHERE Activo = 0);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Corrección de datos manual: no se revierte automáticamente.
        }
    }
}
