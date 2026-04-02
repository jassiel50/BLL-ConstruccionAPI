using BLL_ConstruccionAPI.DTOs.Materiales;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;

namespace BLL_ConstruccionAPI.Services;

public class MaterialesService : IMaterialesService
{
    private readonly IMaterialesRepository _materialesRepo;
    private readonly ICatalogosRepository _catalogosRepo;

    public MaterialesService(IMaterialesRepository materialesRepo, ICatalogosRepository catalogosRepo)
    {
        _materialesRepo = materialesRepo;
        _catalogosRepo = catalogosRepo;
    }

    public async Task<IEnumerable<Material>> GetAllAsync()
        => await _materialesRepo.GetAllAsync();

    public async Task<IEnumerable<Material>> GetBajoStockAsync()
        => await _materialesRepo.GetBajoStockAsync();

    public async Task<Material?> GetByIdAsync(int id)
        => await _materialesRepo.GetByIdAsync(id);

    public async Task<AlmacenCentral?> GetStockAsync(int materialId)
        => await _materialesRepo.GetStockCentralAsync(materialId);

    public async Task<(bool Success, string Message, Material? Data)> CreateAsync(MaterialRequestDto dto)
    {
        if (await _materialesRepo.ExisteCodigoAsync(dto.Codigo))
            return (false, "Ya existe un material con ese código.", null);

        var categoria = await _catalogosRepo.GetCategoriaByIdAsync(dto.CategoriaId);
        if (categoria is null || !categoria.Activo)
            return (false, "La categoría especificada no existe o está inactiva.", null);

        var unidad = await _catalogosRepo.GetUnidadByIdAsync(dto.UnidadMedidaId);
        if (unidad is null || !unidad.Activo)
            return (false, "La unidad de medida especificada no existe o está inactiva.", null);

        var material = new Material
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Codigo = dto.Codigo,
            CategoriaId = dto.CategoriaId,
            UnidadMedidaId = dto.UnidadMedidaId,
            StockMinimo = dto.StockMinimo,
            PrecioUnitario = dto.PrecioUnitario,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        await _materialesRepo.CreateAsync(material);

        // Inicializar registro en AlmacenCentral con Stock = 0
        await _materialesRepo.CreateStockCentralAsync(new AlmacenCentral
        {
            MaterialId = material.Id,
            Stock = 0,
            UltimaActualizacion = DateTime.UtcNow
        });

        return (true, "Material registrado correctamente.", material);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, MaterialRequestDto dto)
    {
        var material = await _materialesRepo.GetByIdAsync(id);
        if (material is null) return (false, "Material no encontrado.");

        if (material.Codigo != dto.Codigo && await _materialesRepo.ExisteCodigoAsync(dto.Codigo))
            return (false, "Ya existe un material con ese código.");

        var categoria = await _catalogosRepo.GetCategoriaByIdAsync(dto.CategoriaId);
        if (categoria is null || !categoria.Activo)
            return (false, "La categoría especificada no existe o está inactiva.");

        var unidad = await _catalogosRepo.GetUnidadByIdAsync(dto.UnidadMedidaId);
        if (unidad is null || !unidad.Activo)
            return (false, "La unidad de medida especificada no existe o está inactiva.");

        material.Nombre = dto.Nombre;
        material.Descripcion = dto.Descripcion;
        material.Codigo = dto.Codigo;
        material.CategoriaId = dto.CategoriaId;
        material.UnidadMedidaId = dto.UnidadMedidaId;
        material.StockMinimo = dto.StockMinimo;
        material.PrecioUnitario = dto.PrecioUnitario;

        await _materialesRepo.UpdateAsync(material);
        return (true, "Material actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var material = await _materialesRepo.GetByIdAsync(id);
        if (material is null) return (false, "Material no encontrado.");

        await _materialesRepo.DeleteAsync(material);
        return (true, "Material desactivado correctamente.");
    }
}
