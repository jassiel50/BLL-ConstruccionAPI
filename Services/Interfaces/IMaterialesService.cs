using BLL_ConstruccionAPI.DTOs.Materiales;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IMaterialesService
{
    Task<IEnumerable<MaterialResponseDto>> GetAllAsync();
    Task<IEnumerable<MaterialResponseDto>> GetBajoStockAsync();
    Task<MaterialResponseDto?> GetByIdAsync(int id);
    Task<AlmacenCentralResponseDto?> GetStockAsync(int materialId);
    Task<(bool Success, string Message, MaterialResponseDto? Data)> CreateAsync(MaterialRequestDto dto);
    Task<(bool Success, string Message)> UpdateAsync(int id, MaterialRequestDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
