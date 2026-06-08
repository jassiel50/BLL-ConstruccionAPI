namespace BLL_ConstruccionAPI.Models.Auth;

public class RegistroNotificacionFase
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }         // fecha local México del día chequeado
    public string Tipo { get; set; } = "";      // "hoy" | "mañana"
    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
}
