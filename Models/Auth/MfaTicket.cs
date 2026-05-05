namespace BLL_ConstruccionAPI.Models.Auth;

public class MfaTicket
{
    public int UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
}
