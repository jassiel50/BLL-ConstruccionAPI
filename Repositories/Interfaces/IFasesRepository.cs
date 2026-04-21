using BLL_ConstruccionAPI.Models.Inventario.Proyectos;

namespace BLL_ConstruccionAPI.Repositories.Interfaces;

public interface IFasesRepository
{
    Task<List<FaseProyecto>> GetByProyectoAsync(int proyectoId);
    Task<FaseProyecto?> GetByIdAsync(int id);
    Task<List<FaseProyecto>> GetAtrasadasAsync();
    Task<int> CreateAsync(FaseProyecto fase);
    Task UpdateAsync(FaseProyecto fase);
    Task DeleteAsync(FaseProyecto fase);
}
