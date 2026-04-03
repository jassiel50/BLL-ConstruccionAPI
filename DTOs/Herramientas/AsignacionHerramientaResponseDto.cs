using BLL_ConstruccionAPI.Models.Inventario.Herramientas;

namespace BLL_ConstruccionAPI.DTOs.Herramientas;

public class AsignacionHerramientaResponseDto
{
    public int Id { get; set; }
    public int HerramientaId { get; set; }
    public string NombreHerramienta { get; set; } = string.Empty;
    public int ProyectoId { get; set; }
    public string NombreProyecto { get; set; } = string.Empty;
    public int UsuarioAsignoId { get; set; }
    public int? UsuarioRecibeId { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public string? ObservacionesDevolucion { get; set; }

    public static AsignacionHerramientaResponseDto FromEntity(AsignacionHerramienta e) => new()
    {
        Id = e.Id,
        HerramientaId = e.HerramientaId,
        NombreHerramienta = e.Herramienta?.Nombre ?? string.Empty,
        ProyectoId = e.ProyectoId,
        NombreProyecto = e.Proyecto?.Nombre ?? string.Empty,
        UsuarioAsignoId = e.UsuarioAsignoId,
        UsuarioRecibeId = e.UsuarioRecibeId,
        FechaAsignacion = e.FechaAsignacion,
        FechaDevolucion = e.FechaDevolucion,
        Estado = e.Estado.ToString(),
        Observaciones = e.Observaciones,
        ObservacionesDevolucion = e.ObservacionesDevolucion
    };
}
