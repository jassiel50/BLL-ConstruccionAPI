namespace BLL_ConstruccionAPI.Models.Auth;

public class Usuario2FA
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string SecretKey { get; set; } = string.Empty;
    public bool Habilitado { get; set; } = false;
    public DateTime? FechaActivado { get; set; }
}
