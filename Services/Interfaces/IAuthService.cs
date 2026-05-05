using BLL_ConstruccionAPI.DTOs.Auth;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto dto, string ipOrigen);
    Task<(bool Success, string Message, LoginResponseDto? Data)> LoginAsync(LoginRequestDto dto, string ipOrigen);

    // Legacy 2FA (mantener compatibilidad)
    Task<(bool Success, string Message, LoginResponseDto? Data)> Verify2FAAsync(Verify2FARequestDto dto, string ipOrigen);
    Task<(bool Success, string Message, Enable2FAResponseDto? Data)> Enable2FAAsync(int usuarioId);
    Task<(bool Success, string Message)> Confirm2FAAsync(Verify2FARequestDto dto);

    // Nuevos métodos MFA flexible
    Task<(bool Success, string Message, Enable2FAResponseDto? Data)> SelectMfaMethodAsync(SelectMfaMethodRequestDto dto, string ipOrigen);
    Task<(bool Success, string Message)> SendMfaEmailCodeAsync(SendMfaEmailCodeRequestDto dto, string ipOrigen);
    Task<(bool Success, string Message, LoginResponseDto? Data)> VerifyMfaAsync(VerifyMfaRequestDto dto, string ipOrigen);
    Task<(bool Success, string Message, Enable2FAResponseDto? Data)> ConfirmMfaAppSetupAsync(ConfirmMfaAppSetupRequestDto dto);
    Task<(bool Success, string Message, Enable2FAResponseDto? Data)> ChangeMfaMethodAsync(ChangeMfaMethodRequestDto dto, int usuarioId, string ipOrigen);

    // Password Reset
    Task<(bool Success, string Message)> RequestPasswordResetAsync(RequestPasswordResetDto dto, string ipOrigen);
    Task<(bool Success, string Message)> VerifyPasswordResetCodeAsync(VerifyPasswordResetDto dto, string ipOrigen);
    Task<(bool Success, string Message)> ConfirmPasswordResetAsync(ConfirmPasswordResetDto dto, string ipOrigen);

    // Tokens
    Task<(bool Success, string Message, LoginResponseDto? Data)> RefreshTokenAsync(RefreshTokenRequestDto dto);
    Task<(bool Success, string Message)> LogoutAsync(RefreshTokenRequestDto dto);
}