namespace BLL_ConstruccionAPI.Models.Auth;

public class PasswordResetCode
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string CodigoHash { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaExpira { get; set; }
    public int IntentosFallidos { get; set; } = 0;
    public int MaxIntentos { get; set; } = 5;
    public bool Usado { get; set; } = false;
    public string? IpSolicitud { get; set; }
    public string? IpVerificacion { get; set; }

    // Relación
    public Usuario? Usuario { get; set; }
}
