using BLL_ConstruccionAPI.DTOs.GastosSemanales;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IGastoSemanalService
{
    Task<List<GastoSemanalDto>> GetByProyectoAsync(int proyectoId);
    Task<(bool Success, GastoSemanalDto? Data)> CreateAsync(int proyectoId, GastoSemanalRequestDto dto);
    Task<bool> DeleteAsync(int id);
    Task<(bool Found, GastoSemanalDto? Data)> GetUltimoAsync(int proyectoId);
}
