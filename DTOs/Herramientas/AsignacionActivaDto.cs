using BLL_ConstruccionAPI.Models.Inventario.Herramientas;

namespace BLL_ConstruccionAPI.DTOs.Herramientas;

public class AsignacionActivaDto
{
    public int Id { get; set; }
    public int HerramientaId { get; set; }
    public string NombreHerramienta { get; set; } = string.Empty;
    public string CodigoHerramienta { get; set; } = string.Empty;
    public int ProyectoId { get; set; }
    public string NombreProyecto { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public string TipoUbicacion { get; set; } = string.Empty;
    public DateTime FechaAsignacion { get; set; }
    public int? UsuarioRecibeId { get; set; }
    public string Observaciones { get; set; } = string.Empty;

    public static AsignacionActivaDto FromEntity(AsignacionHerramienta e) => new()
    {
        Id = e.Id,
        HerramientaId = e.HerramientaId,
        NombreHerramienta = e.Herramienta?.Nombre ?? string.Empty,
        CodigoHerramienta = e.Herramienta?.Codigo ?? string.Empty,
        ProyectoId = e.ProyectoId,
        NombreProyecto = e.Proyecto?.Nombre ?? string.Empty,
        Zona = e.Herramienta?.Zona.ToString() ?? string.Empty,
        TipoUbicacion = e.Herramienta?.TipoUbicacion.ToString() ?? string.Empty,
        FechaAsignacion = e.FechaAsignacion,
        UsuarioRecibeId = e.UsuarioRecibeId,
        Observaciones = e.Observaciones
    };
}
