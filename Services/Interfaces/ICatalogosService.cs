using BLL_ConstruccionAPI.DTOs.Catalogos;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface ICatalogosService
{
    // CategoriaProveedor
    Task<IEnumerable<CategoriaProveedorResponseDto>> GetAllCategoriasProveedorAsync();
    Task<CategoriaProveedorResponseDto?> GetCategoriaProveedorByIdAsync(int id);
    Task<(bool Success, string Message, CategoriaProveedorResponseDto? Data)> CreateCategoriaProveedorAsync(CategoriaProveedorRequestDto dto);
    Task<(bool Success, string Message)> UpdateCategoriaProveedorAsync(int id, CategoriaProveedorRequestDto dto);
    Task<(bool Success, string Message)> DeleteCategoriaProveedorAsync(int id);

    // CategoriaCliente
    Task<IEnumerable<CategoriaClienteResponseDto>> GetAllCategoriasClienteAsync();
    Task<CategoriaClienteResponseDto?> GetCategoriaClienteByIdAsync(int id);
    Task<(bool Success, string Message, CategoriaClienteResponseDto? Data)> CreateCategoriaClienteAsync(CategoriaClienteRequestDto dto);
    Task<(bool Success, string Message)> UpdateCategoriaClienteAsync(int id, CategoriaClienteRequestDto dto);
    Task<(bool Success, string Message)> DeleteCategoriaClienteAsync(int id);


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
