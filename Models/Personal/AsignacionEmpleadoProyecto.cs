using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;

namespace BLL_ConstruccionAPI.Models.Personal;

public class AsignacionEmpleadoProyecto
{
    public int Id { get; set; }
    public int EmpleadoId { get; set; }
    public int ProyectoId { get; set; }
    public int UsuarioAsignoId { get; set; }

    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaFin { get; set; }
    public EstadoAsignacionEmpleado Estado { get; set; } = EstadoAsignacionEmpleado.Activa;

    public string? Observaciones { get; set; }
    public string? ObservacionesFin { get; set; }

    // Navegación
    public Empleado? Empleado { get; set; }
    public Proyecto? Proyecto { get; set; }
}
