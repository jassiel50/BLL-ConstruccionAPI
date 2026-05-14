using BLL_ConstruccionAPI.DTOs.Fases;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IFasesService
{
    Task<List<FaseResponseDto>> GetByProyectoAsync(int proyectoId);
    Task<(bool Success, string Message, FaseResponseDto? Data)> CreateAsync(int proyectoId, FaseRequestDto dto);
    Task<(bool Success, string Message)> UpdateAsync(int id, FaseRequestDto dto);
    Task<(bool Success, string Message)> CompletarAsync(int id);
    Task<(bool Success, string Message)> DescompletarAsync(int id);
    Task<(bool Success, string Message)> DeleteAsync(int id);
    Task<List<FaseResponseDto>> GetAtrasadasAsync();
    Task<List<FaseResponseDto>> GetPorVencerAsync();
    Task<(bool Success, string Message, List<FaseResponseDto> Data)> CrearPlaneacionInicialAsync(int proyectoId, PlaneacionInicialRequestDto dto);
}
