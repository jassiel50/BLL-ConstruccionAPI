namespace BLL_ConstruccionAPI.DTOs.Auth;

public class VerifyMfaRequestDto
{
    public string MfaTicket { get; set; } = string.Empty;
    public string Metodo { get; set; } = string.Empty; // "app" | "email"
    public string Codigo { get; set; } = string.Empty;
}
