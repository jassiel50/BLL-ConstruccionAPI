namespace BLL_ConstruccionAPI.DTOs.Auth;

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
    public bool Requiere2FA { get; set; }
    public bool Configurado2FA { get; set; }
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
}

