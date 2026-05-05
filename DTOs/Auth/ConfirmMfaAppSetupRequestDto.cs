namespace BLL_ConstruccionAPI.DTOs.Auth;

public class ConfirmMfaAppSetupRequestDto
{
    public string MfaTicket { get; set; } = string.Empty;
    public string CodigoTotp { get; set; } = string.Empty;
}
