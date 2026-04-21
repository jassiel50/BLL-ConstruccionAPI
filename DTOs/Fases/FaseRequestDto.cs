namespace BLL_ConstruccionAPI.DTOs.Fases;

public class FaseRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Orden { get; set; }
    public DateTime FechaLimite { get; set; }
}
