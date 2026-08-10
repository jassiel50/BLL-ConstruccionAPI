using BLL_ConstruccionAPI.DTOs.GastosSemanales;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IGastoSemanalService
{
    Task<List<GastoSemanalDto>> GetByProyectoAsync(int proyectoId);
    Task<(bool Success, GastoSemanalDto? Data)> CreateAsync(int proyectoId, GastoSemanalRequestDto dto);
    Task<bool> DeleteAsync(int id);
    Task<(bool Found, GastoSemanalDto? Data)> GetUltimoAsync(int proyectoId);

    Task<List<NominaDisponibleProyectoDto>> GetNominaDisponibleParaProyectoAsync(int proyectoId);
    Task<(bool Success, string Message, GastoSemanalDto? Data)> CrearDesdeNominaAsync(int proyectoId, int periodoNominaId);

    /// <summary>
    /// Crea el GastoSemanal de este proyecto+periodo automáticamente, solo si todos los
    /// NominaDetalle de ese proyecto en ese periodo ya están pagados y aún no se había asociado.
    /// Es un no-op silencioso si falta algún pago o si ya existe (idempotente).
    /// </summary>
    Task AsociarSiCompletoAsync(int proyectoId, int periodoNominaId);
}
