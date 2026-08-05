using BLL_ConstruccionAPI.DTOs.Nomina;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface INominaService
{
    Task<List<PeriodoNominaDto>> GetPeriodosAsync();
    Task<PeriodoNominaDto?> GetPeriodoByIdAsync(int id);
    Task<(bool Success, string Message, PeriodoNominaDto? Data)> GenerarPeriodoAsync(GenerarPeriodoNominaRequestDto dto, int usuarioId);
    Task<(bool Success, string Message)> MarcarPeriodoPagadoAsync(int periodoId);
    Task<(bool Success, string Message)> MarcarDetallePagadoAsync(int detalleId);

    Task<List<HistorialNominaEmpleadoDto>> GetHistorialEmpleadoAsync(int empleadoId);
    Task<List<CostoProyectoNominaDto>> GetCostosPorProyectoAsync();

    Task<byte[]> GenerarReportePdfAsync(int periodoId);
}
