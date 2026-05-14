namespace BLL_ConstruccionAPI.Models.Inventario.Proyectos;

public class ChecklistItem
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int? FaseId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public bool Completado { get; set; } = false;
    public DateTime? FechaCompletado { get; set; }
    public int? CompletadoPorId { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public Proyecto? Proyecto { get; set; }
    public FaseProyecto? Fase { get; set; }
}
