using BLL_ConstruccionAPI.DTOs.Devoluciones;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IDevolucionesMaterialService
{
    Task<IEnumerable<DevolucionMaterialResponseDto>> GetAllAsync();
    Task<IEnumerable<DevolucionMaterialResponseDto>> GetByProyectoAsync(int proyectoId);
    Task<DevolucionMaterialResponseDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, DevolucionMaterialResponseDto? Data)> CreateAsync(int usuarioId, DevolucionMaterialRequestDto dto);
}
