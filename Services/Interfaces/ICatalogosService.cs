using BLL_ConstruccionAPI.DTOs.Catalogos;
using BLL_ConstruccionAPI.Models.Inventario.Cátalogos;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface ICatalogosService
{
    // ─── CategoriaMaterial ────────────────────────────────────────────────────
    Task<IEnumerable<CategoriaMaterial>> GetAllCategoriasAsync();
    Task<CategoriaMaterial?> GetCategoriaByIdAsync(int id);
    Task<(bool Success, string Message, CategoriaMaterial? Data)> CreateCategoriaAsync(CategoriaMaterialRequestDto dto);
    Task<(bool Success, string Message)> UpdateCategoriaAsync(int id, CategoriaMaterialRequestDto dto);
    Task<(bool Success, string Message)> DeleteCategoriaAsync(int id);

    // ─── CategoriaHerramienta ─────────────────────────────────────────────────
    Task<IEnumerable<CategoriaHerramienta>> GetAllCategoriasHerramientaAsync();
    Task<CategoriaHerramienta?> GetCategoriaHerramientaByIdAsync(int id);
    Task<(bool Success, string Message, CategoriaHerramienta? Data)> CreateCategoriaHerramientaAsync(CategoriaHerramientaRequestDto dto);
    Task<(bool Success, string Message)> UpdateCategoriaHerramientaAsync(int id, CategoriaHerramientaRequestDto dto);
    Task<(bool Success, string Message)> DeleteCategoriaHerramientaAsync(int id);

    // ─── UnidadMedida ─────────────────────────────────────────────────────────
    Task<IEnumerable<UnidadMedida>> GetAllUnidadesAsync();
    Task<UnidadMedida?> GetUnidadByIdAsync(int id);
    Task<(bool Success, string Message, UnidadMedida? Data)> CreateUnidadAsync(UnidadMedidaRequestDto dto);
    Task<(bool Success, string Message)> UpdateUnidadAsync(int id, UnidadMedidaRequestDto dto);
    Task<(bool Success, string Message)> DeleteUnidadAsync(int id);
}
