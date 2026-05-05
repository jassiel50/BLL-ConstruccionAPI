namespace BLL_ConstruccionAPI.DTOs.Auth;

public class SelectMfaMethodRequestDto
{
    public string MfaTicket { get; set; } = string.Empty;
    public string Metodo { get; set; } = string.Empty; // "app" | "email"
}
