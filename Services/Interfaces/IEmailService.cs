namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendMfaCodeAsync(string toEmail, string userName, string code, int expirationMinutes);
    Task<bool> SendPasswordResetCodeAsync(string toEmail, string userName, string code, int expirationMinutes);
}
