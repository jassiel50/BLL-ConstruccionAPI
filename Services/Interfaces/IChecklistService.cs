using BLL_ConstruccionAPI.DTOs.Checklist;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IChecklistService
{
    Task<List<ChecklistItemDto>> GetByProyectoAsync(int proyectoId);
    Task<List<ChecklistItemDto>> GetByFaseAsync(int faseId);
    Task<(bool Success, string Message, ChecklistItemDto? Data)> CreateAsync(int proyectoId, ChecklistItemRequestDto dto);
    Task<(bool Success, string Message)> CompletarAsync(int id);
    Task<(bool Success, string Message)> DescompletarAsync(int id);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
