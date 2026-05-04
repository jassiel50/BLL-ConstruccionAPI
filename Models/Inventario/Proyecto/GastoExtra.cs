namespace BLL_ConstruccionAPI.Models.Inventario.Proyectos;

public class GastoExtra
{
    public int Id { get; set; }
    public int FaseId { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Observaciones { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Navegación
    public FaseProyecto? Fase { get; set; }
}
