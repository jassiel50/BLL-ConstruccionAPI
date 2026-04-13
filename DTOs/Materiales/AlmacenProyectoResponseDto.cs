using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.DTOs.Materiales;

public class AlmacenProyectoResponseDto
{
    public int ProyectoId { get; set; }
    public int MaterialId { get; set; }
    public string NombreMaterial { get; set; } = string.Empty;
    public string CodigoMaterial { get; set; } = string.Empty;
    public decimal Stock { get; set; }
    public string Zona { get; set; } = string.Empty;
    public string TipoUbicacion { get; set; } = string.Empty;
    public DateTime UltimaActualizacion { get; set; }

    public static AlmacenProyectoResponseDto FromEntity(AlmacenProyecto e) => new()
    {
        ProyectoId = e.ProyectoId,
        MaterialId = e.MaterialId,
        NombreMaterial = e.Material?.Nombre ?? string.Empty,
        CodigoMaterial = e.Material?.Codigo ?? string.Empty,
        Stock = e.Stock,
        Zona = e.Zona.ToString(),
        TipoUbicacion = e.TipoUbicacion.ToString(),
        UltimaActualizacion = e.UltimaActualizacion
    };
}
