using BLL_ConstruccionAPI.Models.Personal;

namespace BLL_ConstruccionAPI.DTOs.Personal;

public class AsignacionEmpleadoRequestDto
{
    public int ProyectoId { get; set; }
    public string? Observaciones { get; set; }
}

public class AsignacionEmpleadoFinalizarDto
{
    public string? ObservacionesFin { get; set; }
}

public class AsignacionEmpleadoResponseDto
{
    public int Id { get; set; }
    public int EmpleadoId { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public int ProyectoId { get; set; }
    public string ProyectoNombre { get; set; } = string.Empty;

    public DateTime FechaAsignacion { get; set; }
    public DateTime? FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty;

    public string? Observaciones { get; set; }
    public string? ObservacionesFin { get; set; }

    public static AsignacionEmpleadoResponseDto FromEntity(AsignacionEmpleadoProyecto a) => new()
    {
        Id               = a.Id,
        EmpleadoId       = a.EmpleadoId,
        EmpleadoNombre   = a.Empleado?.NombreCompleto ?? string.Empty,
        ProyectoId       = a.ProyectoId,
        ProyectoNombre   = a.Proyecto?.Nombre ?? string.Empty,
        FechaAsignacion  = a.FechaAsignacion,
        FechaFin         = a.FechaFin,
        Estado           = a.Estado.ToString(),
        Observaciones    = a.Observaciones,
        ObservacionesFin = a.ObservacionesFin
    };
}
