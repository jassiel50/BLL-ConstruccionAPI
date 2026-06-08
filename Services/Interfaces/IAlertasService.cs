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
}
