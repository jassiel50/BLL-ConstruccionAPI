namespace BLL_ConstruccionAPI.DTOs.Devoluciones;

public class DevolucionMaterialRequestDto
{
    public int ProyectoId { get; set; }
    public int MaterialId { get; set; }
    public decimal CantidadDevuelta { get; set; }
    public string Observaciones { get; set; } = string.Empty;
}
