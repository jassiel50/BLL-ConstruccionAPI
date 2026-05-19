using BLL_ConstruccionAPI.Models.Inventario.Cátalogos;

namespace BLL_ConstruccionAPI.Repositories.Interfaces;

public interface ICatalogosRepository
{
    // ─── CategoriaProveedor ───────────────────────────────────────────────────
    Task<IEnumerable<CategoriaProveedor>> GetAllCategoriasProveedorAsync();
    Task<CategoriaProveedor?> GetCategoriaProveedorByIdAsync(int id);
    Task<bool> ExisteCategoriaProveedorAsync(string nombre);
    Task<int> CreateCategoriaProveedorAsync(CategoriaProveedor categoria);
    Task UpdateCategoriaProveedorAsync(CategoriaProveedor categoria);
    Task DeleteCategoriaProveedorAsync(CategoriaProveedor categoria);

    // ─── CategoriaCliente ─────────────────────────────────────────────────────
    Task<IEnumerable<CategoriaCliente>> GetAllCategoriasClienteAsync();
    Task<CategoriaCliente?> GetCategoriaClienteByIdAsync(int id);
    Task<bool> ExisteCategoriaClienteAsync(string nombre);
    Task<int> CreateCategoriaClienteAsync(CategoriaCliente categoria);
    Task UpdateCategoriaClienteAsync(CategoriaCliente categoria);
    Task DeleteCategoriaClienteAsync(CategoriaCliente categoria);


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
