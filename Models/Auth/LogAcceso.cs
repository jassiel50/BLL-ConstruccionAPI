namespace BLL_ConstruccionAPI.Models.Auth;

public class LogAcceso
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public bool Exitoso { get; set; }
    public string? IpOrigen { get; set; }
    public string? Descripcion { get; set; }
}