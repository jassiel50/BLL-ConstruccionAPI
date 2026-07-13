namespace BLL_ConstruccionAPI.DTOs.Proyectos;

public class HistorialFinancieroItemDto
{
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty; // "Ingreso" | "Gasto"
    public string Categoria { get; set; } = string.Empty; // "Pago", "Material", "Herramienta", "Gasto Extra", "Gasto Semanal"
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? Referencia { get; set; }
}
