using BLL_ConstruccionAPI.DTOs.Herramientas;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IHerramientasService
{
    Task<IEnumerable<HerramientaResponseDto>> GetAllAsync();
    Task<IEnumerable<HerramientaResponseDto>> GetDisponiblesAsync();
    Task<HerramientaResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<AsignacionHerramientaResponseDto>> GetAsignacionesAsync(int herramientaId);
    Task<(bool Success, string Message, HerramientaResponseDto? Data)> CreateAsync(HerramientaRequestDto dto);
    Task<(bool Success, string Message)> UpdateAsync(int id, HerramientaRequestDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
    Task<(bool Success, string Message, AsignacionHerramientaResponseDto? Data)> AsignarAsync(AsignacionRequestDto dto, int usuarioId);
    Task<(bool Success, string Message)> DevolverAsync(int asignacionId, DevolucionRequestDto dto);
}
