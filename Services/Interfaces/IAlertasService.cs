using BLL_ConstruccionAPI.DTOs.Alertas;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IAlertasService
{
    Task<ResumenAlertasDto> GetResumenAsync();
    Task<List<AlertaDto>> GetStockBajoAsync();
    Task<List<AlertaDto>> GetFasesAtrasadasAsync();
    Task<List<AlertaDto>> GetFasesPorVencerAsync();
    Task<List<AlertaDto>> GetProyectosSinFasesAsync();
    Task<List<AlertaDto>> GetHerramientasSinDevolverAsync();
    Task<List<AlertaDto>> GetSinHerramientasDisponiblesAsync();
    Task<List<AlertaDto>> GetProyectosConFasesCompletadasAsync();
    Task<List<AlertaDto>> GetContratosPorVencerAsync();

    /// <summary>
    /// Envía manualmente (fuera del ciclo automático diario) los 3 correos de fases
    /// (vencen hoy, vencen mañana, atrasadas) a un usuario específico, usando el
    /// estado actual de las fases. No depende ni afecta el registro de "ya enviado"
    /// del día que usa el envío automático.
    /// </summary>
    Task<(bool Success, string Message)> ReenviarNotificacionesFasesAsync(int usuarioId);
}
