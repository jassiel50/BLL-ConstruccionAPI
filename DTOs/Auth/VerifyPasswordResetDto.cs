namespace BLL_ConstruccionAPI.DTOs.Auth;

public class VerifyPasswordResetDto
{
    public string Email { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
}
