using BLL_ConstruccionAPI.Models.Inventario.Proyectos;

namespace BLL_ConstruccionAPI.Repositories.Interfaces;

public interface IProyectosRepository
{
    Task<IEnumerable<Proyecto>> GetAllAsync();
    Task<IEnumerable<Proyecto>> GetByClienteAsync(int clienteId);
    Task<Proyecto?> GetByIdAsync(int id);
    Task<int> CreateAsync(Proyecto proyecto);
    Task UpdateAsync(Proyecto proyecto);
    Task DeleteAsync(Proyecto proyecto);
    Task TerminarAsync(Proyecto proyecto);
}
