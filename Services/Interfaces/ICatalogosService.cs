using BLL_ConstruccionAPI.DTOs.Catalogos;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface ICatalogosService
{
    // CategoriaMaterial
    Task<IEnumerable<CategoriaMaterialResponseDto>> GetAllCategoriasAsync();
    Task<CategoriaMaterialResponseDto?> GetCategoriaByIdAsync(int id);
    Task<(bool Success, string Message, CategoriaMaterialResponseDto? Data)> CreateCategoriaAsync(CategoriaMaterialRequestDto dto);
    Task<(bool Success, string Message)> UpdateCategoriaAsync(int id, CategoriaMaterialRequestDto dto);
    Task<(bool Success, string Message)> DeleteCategoriaAsync(int id);

    // CategoriaHerramienta
    Task<IEnumerable<CategoriaHerramientaResponseDto>> GetAllCategoriasHerramientaAsync();
    Task<CategoriaHerramientaResponseDto?> GetCategoriaHerramientaByIdAsync(int id);
    Task<(bool Success, string Message, CategoriaHerramientaResponseDto? Data)> CreateCategoriaHerramientaAsync(CategoriaHerramientaRequestDto dto);
    Task<(bool Success, string Message)> UpdateCategoriaHerramientaAsync(int id, CategoriaHerramientaRequestDto dto);
    Task<(bool Success, string Message)> DeleteCategoriaHerramientaAsync(int id);

    // UnidadMedida
    Task<IEnumerable<UnidadMedidaResponseDto>> GetAllUnidadesAsync();
    Task<UnidadMedidaResponseDto?> GetUnidadByIdAsync(int id);
    Task<(bool Success, string Message, UnidadMedidaResponseDto? Data)> CreateUnidadAsync(UnidadMedidaRequestDto dto);
    Task<(bool Success, string Message)> UpdateUnidadAsync(int id, UnidadMedidaRequestDto dto);
    Task<(bool Success, string Message)> DeleteUnidadAsync(int id);
}
