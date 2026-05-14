using BLL_ConstruccionAPI.DTOs.GastosExtras;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IGastoExtraService
{
    Task<List<GastoExtraDto>> GetByFaseAsync(int faseId);
    Task<(bool Success, GastoExtraDto? Data)> CreateAsync(int faseId, GastoExtraRequestDto dto);
    Task<(bool Success, GastoExtraDto? Data)> UpdateAsync(int id, GastoExtraRequestDto dto);
    Task<bool> DeleteAsync(int id);
}
