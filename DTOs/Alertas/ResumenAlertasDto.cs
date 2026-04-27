namespace BLL_ConstruccionAPI.DTOs.Alertas;

public class ResumenAlertasDto
{
    public int TotalAlertas { get; set; }
    public int StockBajo { get; set; }
    public int FasesAtrasadas { get; set; }
    public int FasesPorVencer { get; set; }
    public int ProyectosSinFases { get; set; }
    public int HerramientasSinDevolver { get; set; }
    public int SinHerramientasDisponibles { get; set; }
    public List<AlertaDto> Detalle { get; set; } = new();
}
