using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class LimpiarAsistenciasHuerfanas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Elimina registros de AsistenciaDiaria huérfanos: quedaron sin periodo asociado
            // porque, antes de este fix, EliminarPeriodoAsync no los limpiaba. Bloqueaban la
            // generación de nuevas nóminas para esas mismas fechas por el índice único
            // (EmpleadoId, Fecha).
            migrationBuilder.Sql(@"
                DELETE a FROM AsistenciasDiarias a
                WHERE NOT EXISTS (
                    SELECT 1 FROM PeriodosNomina p
                    WHERE a.Fecha >= p.FechaInicio AND a.Fecha <= p.FechaFin
                )");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No reversible: los registros eliminados eran datos huérfanos sin valor de negocio.
        }
    }
}
