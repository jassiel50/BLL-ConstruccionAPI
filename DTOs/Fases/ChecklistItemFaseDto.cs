using BLL_ConstruccionAPI.Models.Inventario.Proyectos;

namespace BLL_ConstruccionAPI.DTOs.Fases;

public class ChecklistItemFaseDto
{
    public int Id { get; set; }
    public int FaseId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Completado { get; set; }
    public DateTime? FechaCompletado { get; set; }
    public DateTime FechaCreacion { get; set; }

    public static ChecklistItemFaseDto FromEntity(ChecklistItemFase c) => new()
    {
        Id              = c.Id,
        FaseId          = c.FaseId,
        Descripcion     = c.Descripcion,
        Orden           = c.Orden,
        Completado      = c.Completado,
        FechaCompletado = c.FechaCompletado,
        FechaCreacion   = c.FechaCreacion
    };
}

public class ChecklistItemFaseRequestDto
{
    public string Descripcion { get; set; } = string.Empty;
}
