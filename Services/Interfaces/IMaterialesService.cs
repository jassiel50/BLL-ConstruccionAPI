using BLL_ConstruccionAPI.DTOs.Materiales;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IMaterialesService
{
    Task<IEnumerable<Material>> GetAllAsync();
    Task<IEnumerable<Material>> GetBajoStockAsync();
    Task<Material?> GetByIdAsync(int id);
    Task<AlmacenCentral?> GetStockAsync(int materialId);
    Task<(bool Success, string Message, Material? Data)> CreateAsync(MaterialRequestDto dto);
    Task<(bool Success, string Message)> UpdateAsync(int id, MaterialRequestDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
