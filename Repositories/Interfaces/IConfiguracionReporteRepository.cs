using BLL_ConstruccionAPI.Models.Reportes;

namespace BLL_ConstruccionAPI.Repositories.Interfaces;

public interface IConfiguracionReporteRepository
{
    Task<IEnumerable<ConfiguracionReporte>> GetByUsuarioIdAsync(int usuarioId);
    Task<ConfiguracionReporte?> GetByIdAsync(int id);
    Task<ConfiguracionReporte> CreateAsync(ConfiguracionReporte config);
    Task UpdateAsync(ConfiguracionReporte config);
    Task DeleteAsync(ConfiguracionReporte config);
    Task<IEnumerable<ConfiguracionReporte>> GetActivasAsync();
}
