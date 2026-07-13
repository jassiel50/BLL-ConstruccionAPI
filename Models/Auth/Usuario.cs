namespace BLL_ConstruccionAPI.Models.Auth;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? EmailSecundario { get; set; } // Correo adicional para recibir notificaciones/reportes; no se usa para iniciar sesión
    public string PasswordHash { get; set; } = string.Empty;
    public int RolId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoAcceso { get; set; }

    public Rol? Rol { get; set; }
}