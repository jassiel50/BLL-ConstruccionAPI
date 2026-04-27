using BLL_ConstruccionAPI.DTOs.Perdidas;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IPerdidasService
{
    Task<IEnumerable<RegistroPerdidaResponseDto>> GetAllAsync();
    Task<IEnumerable<RegistroPerdidaResponseDto>> GetByProyectoAsync(int proyectoId);
    Task<IEnumerable<RegistroPerdidaResponseDto>> GetByMaterialAsync(int materialId);
    Task<IEnumerable<RegistroPerdidaResponseDto>> GetByHerramientaAsync(int herramientaId);
    Task<RegistroPerdidaResponseDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, RegistroPerdidaResponseDto? Data)> CreateAsync(int usuarioId, RegistroPerdidaRequestDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
