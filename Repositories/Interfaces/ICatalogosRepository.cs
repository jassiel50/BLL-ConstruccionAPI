using BLL_ConstruccionAPI.Models.Inventario.Cátalogos;

namespace BLL_ConstruccionAPI.Repositories.Interfaces;

public interface ICatalogosRepository
{
    // ─── CategoriaMaterial ────────────────────────────────────────────────────
    Task<IEnumerable<CategoriaMaterial>> GetAllCategoriasAsync();
    Task<CategoriaMaterial?> GetCategoriaByIdAsync(int id);
    Task<bool> ExisteCategoriaAsync(string nombre);
    Task<int> CreateCategoriaAsync(CategoriaMaterial categoria);
    Task UpdateCategoriaAsync(CategoriaMaterial categoria);
    Task DeleteCategoriaAsync(CategoriaMaterial categoria);

    // ─── CategoriaHerramienta ─────────────────────────────────────────────────
    Task<IEnumerable<CategoriaHerramienta>> GetAllCategoriasHerramientaAsync();
    Task<CategoriaHerramienta?> GetCategoriaHerramientaByIdAsync(int id);
    Task<bool> ExisteCategoriaHerramientaAsync(string nombre);
    Task<int> CreateCategoriaHerramientaAsync(CategoriaHerramienta categoria);
    Task UpdateCategoriaHerramientaAsync(CategoriaHerramienta categoria);
    Task DeleteCategoriaHerramientaAsync(CategoriaHerramienta categoria);

    // ─── UnidadMedida ─────────────────────────────────────────────────────────
    Task<IEnumerable<UnidadMedida>> GetAllUnidadesAsync();
    Task<UnidadMedida?> GetUnidadByIdAsync(int id);
    Task<bool> ExisteUnidadAsync(string abreviatura);
    Task<int> CreateUnidadAsync(UnidadMedida unidad);
    Task UpdateUnidadAsync(UnidadMedida unidad);
    Task DeleteUnidadAsync(UnidadMedida unidad);
}
