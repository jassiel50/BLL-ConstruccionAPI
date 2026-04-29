using BLL_ConstruccionAPI.DTOs.Auth;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IUsuariosService
{
    Task<IEnumerable<UsuarioResponseDto>> GetAllAsync();
    Task<UsuarioResponseDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message)> CrearAsync(RegisterRequestDto dto);
    Task<(bool Success, string Message)> ToggleActivoAsync(int id);
}
