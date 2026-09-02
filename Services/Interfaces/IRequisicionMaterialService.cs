using BLL_ConstruccionAPI.DTOs.Proyectos;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IRequisicionMaterialService
{
    Task<List<RequisicionMaterialDto>> GetByProyectoAsync(int proyectoId);
    Task<RequisicionMaterialDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, RequisicionMaterialDto? Data)> CreateAsync(int proyectoId, RequisicionMaterialRequestDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
    Task<(bool Success, string Message, byte[]? Pdf)> GenerarPdfAsync(int id);
}
