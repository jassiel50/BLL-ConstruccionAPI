using BLL_ConstruccionAPI.DTOs.Pagos;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IPagosClienteService
{
    Task<ResumenPagosDto> GetResumenAsync(int proyectoId);
    Task<List<PagoClienteDto>> GetByProyectoAsync(int proyectoId);
    Task<(bool Success, string Message, PagoClienteDto? Data)> CreateAsync(int proyectoId, PagoClienteRequestDto dto);
    Task<(bool Success, string Message)> UpdateAsync(int id, PagoClienteRequestDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
    Task<(bool Success, string Message, byte[]? Pdf)> GenerarPdfAsync(int proyectoId);
}
