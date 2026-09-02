using BLL_ConstruccionAPI.DTOs.Proyectos;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IGastoMaterialService
{
    Task<List<GastoMaterialDto>> GetByProyectoAsync(int proyectoId);
    Task<(bool Success, string Message, GastoMaterialDto? Data)> CreateAsync(int proyectoId, GastoMaterialRequestDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
