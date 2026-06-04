using BLL_ConstruccionAPI.DTOs.Reportes;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IConfiguracionReporteService
{
    Task<IEnumerable<ConfiguracionReporteResponseDto>> GetMisConfiguracionesAsync(int usuarioId);
    Task<ConfiguracionReporteResponseDto?> GetByIdAsync(int id, int usuarioId);
    Task<(bool Success, string Message, ConfiguracionReporteResponseDto? Data)> CreateAsync(ConfiguracionReporteRequestDto dto, int usuarioId);
    Task<(bool Success, string Message)> UpdateAsync(int id, ConfiguracionReporteRequestDto dto, int usuarioId);
    Task<(bool Success, string Message)> DeleteAsync(int id, int usuarioId);
    Task<(bool Success, string Message)> EnviarAhoraAsync(int id, int usuarioId);
}
