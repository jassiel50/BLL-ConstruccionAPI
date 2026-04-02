using BLL_ConstruccionAPI.DTOs.Auth;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto dto, string ipOrigen);
    Task<(bool Success, string Message, LoginResponseDto? Data)> LoginAsync(LoginRequestDto dto, string ipOrigen);
    Task<(bool Success, string Message, LoginResponseDto? Data)> Verify2FAAsync(Verify2FARequestDto dto, string ipOrigen);
    Task<(bool Success, string Message, LoginResponseDto? Data)> RefreshTokenAsync(RefreshTokenRequestDto dto);
    Task<(bool Success, string Message)> LogoutAsync(RefreshTokenRequestDto dto);
    Task<(bool Success, string Message, Enable2FAResponseDto? Data)> Enable2FAAsync(int usuarioId);
    Task<(bool Success, string Message)> Confirm2FAAsync(Verify2FARequestDto dto);
}