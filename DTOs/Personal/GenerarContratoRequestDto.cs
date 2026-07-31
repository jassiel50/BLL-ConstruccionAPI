namespace BLL_ConstruccionAPI.DTOs.Personal;

public class GenerarContratoRequestDto
{
    public DateTime FechaInicio { get; set; } = DateTime.Today;
    public int DuracionMeses { get; set; } = 3;
}
