namespace BLL_ConstruccionAPI.DTOs.Auth;

public class SendMfaEmailCodeRequestDto
{
    public string MfaTicket { get; set; } = string.Empty;
}
