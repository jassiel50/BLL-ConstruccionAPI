namespace BLL_ConstruccionAPI.Models.Auth;

public class TokenSesion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime FechaExpira { get; set; }
    public bool Revocado { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string? IpOrigen { get; set; }
}