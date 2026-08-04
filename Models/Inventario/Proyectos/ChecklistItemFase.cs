namespace BLL_ConstruccionAPI.Models.Inventario.Proyectos;

public class ChecklistItemFase
{
    public int Id { get; set; }
    public int FaseId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Completado { get; set; }
    public int? CompletadoPorId { get; set; }
    public DateTime? FechaCompletado { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public FaseProyecto? Fase { get; set; }
}
