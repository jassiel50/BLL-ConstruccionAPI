namespace BLL_ConstruccionAPI.Models.Auth;

public class RegistroCorreoSemanal
{
    public int Id { get; set; }
    public DateTime FechaLunes { get; set; }   // lunes de la semana reportada
    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
}
