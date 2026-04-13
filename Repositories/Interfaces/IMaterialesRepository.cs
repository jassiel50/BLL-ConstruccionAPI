using BLL_ConstruccionAPI.Models.Inventario.Materiales;

namespace BLL_ConstruccionAPI.Repositories.Interfaces;

public interface IMaterialesRepository
{
    // ─── Material ─────────────────────────────────────────────────────────────
    Task<IEnumerable<Material>> GetAllAsync();
    Task<IEnumerable<Material>> GetBajoStockAsync();
    Task<Material?> GetByIdAsync(int id);
    Task<bool> ExisteCodigoAsync(string codigo);
    Task<int> CreateAsync(Material material);
    Task UpdateAsync(Material material);
    Task DeleteAsync(Material material);

    // ─── AlmacenCentral ───────────────────────────────────────────────────────
    Task<IEnumerable<AlmacenCentral>> GetAllStockCentralAsync();
    Task<AlmacenCentral?> GetStockCentralAsync(int materialId);
    Task CreateStockCentralAsync(AlmacenCentral almacen);
    Task UpdateStockCentralAsync(AlmacenCentral almacen);
}
