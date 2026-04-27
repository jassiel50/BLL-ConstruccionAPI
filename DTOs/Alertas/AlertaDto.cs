namespace BLL_ConstruccionAPI.DTOs.Alertas;

public class AlertaDto
{
    public string Tipo { get; set; } = string.Empty;
    // Valores posibles: "StockBajo", "StockCritico", "FaseAtrasada",
    // "FasePorVencer", "ProyectoSinFases", "HerramientaSinDevolver", "SinHerramientasDisponibles"
    public string Severidad { get; set; } = string.Empty;
    // Valores posibles: "Alta", "Media"
    public string Mensaje { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    // Ruta de navegación ej: "/proyectos/1", "/materiales", "/herramientas"
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
