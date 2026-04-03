using BLL_ConstruccionAPI.DTOs.Catalogos;
using BLL_ConstruccionAPI.Models.Inventario.Cátalogos;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;

namespace BLL_ConstruccionAPI.Services;

public class CatalogosService : ICatalogosService
{
    private readonly ICatalogosRepository _catalogosRepo;

    public CatalogosService(ICatalogosRepository catalogosRepo)
    {
        _catalogosRepo = catalogosRepo;
    }

    // ─── CategoriaMaterial ────────────────────────────────────────────────────
    public async Task<IEnumerable<CategoriaMaterialResponseDto>> GetAllCategoriasAsync()
    {
        var categorias = await _catalogosRepo.GetAllCategoriasAsync();
        return categorias.Select(CategoriaMaterialResponseDto.FromEntity);
    }

    public async Task<CategoriaMaterialResponseDto?> GetCategoriaByIdAsync(int id)
    {
        var categoria = await _catalogosRepo.GetCategoriaByIdAsync(id);
        return categoria is null ? null : CategoriaMaterialResponseDto.FromEntity(categoria);
    }

    public async Task<(bool Success, string Message, CategoriaMaterialResponseDto? Data)> CreateCategoriaAsync(CategoriaMaterialRequestDto dto)
    {
        if (await _catalogosRepo.ExisteCategoriaAsync(dto.Nombre))
            return (false, "Ya existe una categoría con ese nombre.", null);

        var categoria = new CategoriaMaterial
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Activo = true
        };

        await _catalogosRepo.CreateCategoriaAsync(categoria);
        return (true, "Categoría creada correctamente.", CategoriaMaterialResponseDto.FromEntity(categoria));
    }

    public async Task<(bool Success, string Message)> UpdateCategoriaAsync(int id, CategoriaMaterialRequestDto dto)
    {
        var categoria = await _catalogosRepo.GetCategoriaByIdAsync(id);
        if (categoria is null) return (false, "Categoría no encontrada.");

        var duplicado = await _catalogosRepo.ExisteCategoriaAsync(dto.Nombre);
        if (duplicado && categoria.Nombre != dto.Nombre)
            return (false, "Ya existe una categoría con ese nombre.");

        categoria.Nombre = dto.Nombre;
        categoria.Descripcion = dto.Descripcion;

        await _catalogosRepo.UpdateCategoriaAsync(categoria);
        return (true, "Categoría actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteCategoriaAsync(int id)
    {
        var categoria = await _catalogosRepo.GetCategoriaByIdAsync(id);
        if (categoria is null) return (false, "Categoría no encontrada.");

        await _catalogosRepo.DeleteCategoriaAsync(categoria);
        return (true, "Categoría desactivada correctamente.");
    }

    // ─── CategoriaHerramienta ─────────────────────────────────────────────────
    public async Task<IEnumerable<CategoriaHerramientaResponseDto>> GetAllCategoriasHerramientaAsync()
    {
        var categorias = await _catalogosRepo.GetAllCategoriasHerramientaAsync();
        return categorias.Select(CategoriaHerramientaResponseDto.FromEntity);
    }

    public async Task<CategoriaHerramientaResponseDto?> GetCategoriaHerramientaByIdAsync(int id)
    {
        var categoria = await _catalogosRepo.GetCategoriaHerramientaByIdAsync(id);
        return categoria is null ? null : CategoriaHerramientaResponseDto.FromEntity(categoria);
    }

    public async Task<(bool Success, string Message, CategoriaHerramientaResponseDto? Data)> CreateCategoriaHerramientaAsync(CategoriaHerramientaRequestDto dto)
    {
        if (await _catalogosRepo.ExisteCategoriaHerramientaAsync(dto.Nombre))
            return (false, "Ya existe una categoría de herramienta con ese nombre.", null);

        var categoria = new CategoriaHerramienta
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Activo = true
        };

        await _catalogosRepo.CreateCategoriaHerramientaAsync(categoria);
        return (true, "Categoría de herramienta creada correctamente.", CategoriaHerramientaResponseDto.FromEntity(categoria));
    }

    public async Task<(bool Success, string Message)> UpdateCategoriaHerramientaAsync(int id, CategoriaHerramientaRequestDto dto)
    {
        var categoria = await _catalogosRepo.GetCategoriaHerramientaByIdAsync(id);
        if (categoria is null) return (false, "Categoría de herramienta no encontrada.");

        var duplicado = await _catalogosRepo.ExisteCategoriaHerramientaAsync(dto.Nombre);
        if (duplicado && categoria.Nombre != dto.Nombre)
            return (false, "Ya existe una categoría de herramienta con ese nombre.");

        categoria.Nombre = dto.Nombre;
        categoria.Descripcion = dto.Descripcion;

        await _catalogosRepo.UpdateCategoriaHerramientaAsync(categoria);
        return (true, "Categoría de herramienta actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteCategoriaHerramientaAsync(int id)
    {
        var categoria = await _catalogosRepo.GetCategoriaHerramientaByIdAsync(id);
        if (categoria is null) return (false, "Categoría de herramienta no encontrada.");

        await _catalogosRepo.DeleteCategoriaHerramientaAsync(categoria);
        return (true, "Categoría de herramienta desactivada correctamente.");
    }

    // ─── UnidadMedida ─────────────────────────────────────────────────────────
    public async Task<IEnumerable<UnidadMedidaResponseDto>> GetAllUnidadesAsync()
    {
        var unidades = await _catalogosRepo.GetAllUnidadesAsync();
        return unidades.Select(UnidadMedidaResponseDto.FromEntity);
    }

    public async Task<UnidadMedidaResponseDto?> GetUnidadByIdAsync(int id)
    {
        var unidad = await _catalogosRepo.GetUnidadByIdAsync(id);
        return unidad is null ? null : UnidadMedidaResponseDto.FromEntity(unidad);
    }

    public async Task<(bool Success, string Message, UnidadMedidaResponseDto? Data)> CreateUnidadAsync(UnidadMedidaRequestDto dto)
    {
        if (await _catalogosRepo.ExisteUnidadAsync(dto.Abreviatura))
            return (false, "Ya existe una unidad de medida con esa abreviatura.", null);

        var unidad = new UnidadMedida
        {
            Nombre = dto.Nombre,
            Abreviatura = dto.Abreviatura,
            Activo = true
        };

        await _catalogosRepo.CreateUnidadAsync(unidad);
        return (true, "Unidad de medida creada correctamente.", UnidadMedidaResponseDto.FromEntity(unidad));
    }

    public async Task<(bool Success, string Message)> UpdateUnidadAsync(int id, UnidadMedidaRequestDto dto)
    {
        var unidad = await _catalogosRepo.GetUnidadByIdAsync(id);
        if (unidad is null) return (false, "Unidad de medida no encontrada.");

        var duplicado = await _catalogosRepo.ExisteUnidadAsync(dto.Abreviatura);
        if (duplicado && unidad.Abreviatura != dto.Abreviatura)
            return (false, "Ya existe una unidad de medida con esa abreviatura.");

        unidad.Nombre = dto.Nombre;
        unidad.Abreviatura = dto.Abreviatura;

        await _catalogosRepo.UpdateUnidadAsync(unidad);
        return (true, "Unidad de medida actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteUnidadAsync(int id)
    {
        var unidad = await _catalogosRepo.GetUnidadByIdAsync(id);
        if (unidad is null) return (false, "Unidad de medida no encontrada.");

        await _catalogosRepo.DeleteUnidadAsync(unidad);
        return (true, "Unidad de medida desactivada correctamente.");
    }
}
