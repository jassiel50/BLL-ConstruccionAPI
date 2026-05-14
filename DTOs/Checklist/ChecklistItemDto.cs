namespace BLL_ConstruccionAPI.DTOs.Checklist;

public class ChecklistItemDto
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int? FaseId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public bool Completado { get; set; }
    public DateTime? FechaCompletado { get; set; }
    public DateTime FechaRegistro { get; set; }
}

public class ChecklistItemRequestDto
{
    public int? FaseId { get; set; }
    public string Titulo { get; set; } = string.Empty;
}
