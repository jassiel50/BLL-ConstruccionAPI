using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Personal;

namespace BLL_ConstruccionAPI.Models.Nomina;

// Solo se guarda una fila cuando el día NO es el default (Estado != Presente o
// HorasExtra > 0). Ausencia de fila para un (EmpleadoId, Fecha) = presente, sin horas extra.
public class AsistenciaDiaria
{
    public int Id { get; set; }

    public int EmpleadoId { get; set; }
    public DateTime Fecha { get; set; }
    public EstadoAsistencia Estado { get; set; } = EstadoAsistencia.Presente;
    public decimal HorasExtra { get; set; }

    public Empleado? Empleado { get; set; }
}
