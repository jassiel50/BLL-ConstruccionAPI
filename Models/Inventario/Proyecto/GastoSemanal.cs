namespace BLL_ConstruccionAPI.Models.Inventario.Proyectos;

public class GastoSemanal
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Tipo { get; set; } = "Semanal";
    public string? Observaciones { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public Proyecto? Proyecto { get; set; }
}
