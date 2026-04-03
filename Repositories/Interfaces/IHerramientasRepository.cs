using BLL_ConstruccionAPI.Models.Inventario.Herramientas;

namespace BLL_ConstruccionAPI.Repositories.Interfaces;

public interface IHerramientasRepository
{
    // ─── Herramienta ─────────────────────────────────────────────────────────
    Task<IEnumerable<Herramienta>> GetAllAsync();
    Task<IEnumerable<Herramienta>> GetDisponiblesAsync();
    Task<Herramienta?> GetByIdAsync(int id);
    Task<bool> ExisteCodigoAsync(string codigo);
    Task<bool> ExisteNumeroSerieAsync(string numeroSerie);
    Task<int> CreateAsync(Herramienta herramienta);
    Task UpdateAsync(Herramienta herramienta);
    Task DeleteAsync(Herramienta herramienta);

    // ─── Asignaciones ─────────────────────────────────────────────────────────
    Task<IEnumerable<AsignacionHerramienta>> GetAsignacionesByHerramientaAsync(int herramientaId);
    Task<AsignacionHerramienta?> GetAsignacionByIdAsync(int id);
    Task<AsignacionHerramienta?> GetAsignacionActivaAsync(int herramientaId);
    Task<int> CreateAsignacionAsync(AsignacionHerramienta asignacion);
    Task UpdateAsignacionAsync(AsignacionHerramienta asignacion);

    // ─── Operaciones atómicas ─────────────────────────────────────────────────
    Task AsignarHerramientaAsync(AsignacionHerramienta asignacion, Herramienta herramienta);
    Task DevolverHerramientaAsync(AsignacionHerramienta asignacion, Herramienta herramienta);
}
