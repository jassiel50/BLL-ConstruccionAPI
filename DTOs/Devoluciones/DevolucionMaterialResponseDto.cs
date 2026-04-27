using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.DTOs.Devoluciones;

public class DevolucionMaterialResponseDto
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string NombreProyecto { get; set; } = string.Empty;
    public int MaterialId { get; set; }
    public string NombreMaterial { get; set; } = string.Empty;
    public decimal CantidadDevuelta { get; set; }
    public string Observaciones { get; set; } = string.Empty;
    public DateTime FechaDevolucion { get; set; }

    public static DevolucionMaterialResponseDto FromEntity(DevolucionMaterial d) => new()
    {
        Id               = d.Id,
        ProyectoId       = d.ProyectoId,
        NombreProyecto   = d.Proyecto?.Nombre ?? string.Empty,
        MaterialId       = d.MaterialId,
        NombreMaterial   = d.Material?.Nombre ?? string.Empty,
        CantidadDevuelta = d.CantidadDevuelta,
        Observaciones    = d.Observaciones,
        FechaDevolucion  = d.FechaDevolucion
    };
}
