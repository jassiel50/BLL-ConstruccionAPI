namespace BLL_ConstruccionAPI.DTOs.Auth;

/// <summary>Vista mínima de usuario para selectores (ej. destinatarios de reportes), sin datos sensibles de administración.</summary>
public class UsuarioDirectorioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? EmailSecundario { get; set; }
}
