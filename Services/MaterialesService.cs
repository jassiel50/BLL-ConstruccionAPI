using BLL_ConstruccionAPI.DTOs.Materiales;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Services;

public class MaterialesService : IMaterialesService
{
    private readonly IMaterialesRepository _materialesRepo;
    private readonly ICatalogosRepository _catalogosRepo;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MaterialesService(
        IMaterialesRepository materialesRepo,
        ICatalogosRepository catalogosRepo,
        IBitacoraService bitacora,
        IHttpContextAccessor httpContextAccessor)
    {
        _materialesRepo = materialesRepo;
        _catalogosRepo = catalogosRepo;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<MaterialResponseDto>> GetAllAsync()
    {
        var materiales = await _materialesRepo.GetAllAsync();
        return materiales.Select(MaterialResponseDto.FromEntity);
    }

    public async Task<IEnumerable<MaterialResponseDto>> GetBajoStockAsync()
    {
        var materiales = await _materialesRepo.GetBajoStockAsync();
        return materiales.Select(MaterialResponseDto.FromEntity);
    }

    public async Task<MaterialResponseDto?> GetByIdAsync(int id)
    {
        var material = await _materialesRepo.GetByIdAsync(id);
        return material is null ? null : MaterialResponseDto.FromEntity(material);
    }

    public async Task<IEnumerable<AlmacenCentralResponseDto>> GetAlmacenCentralAsync()
    {
        var registros = await _materialesRepo.GetAllStockCentralAsync();
        return registros.Select(AlmacenCentralResponseDto.FromEntity);
    }

    public async Task<AlmacenCentralResponseDto?> GetStockAsync(int materialId)
    {
        var stock = await _materialesRepo.GetStockCentralAsync(materialId);
        return stock is null ? null : AlmacenCentralResponseDto.FromEntity(stock);
    }

    public async Task<(bool Success, string Message, MaterialResponseDto? Data)> CreateAsync(MaterialRequestDto dto)
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
            Zona = Zona.Guadalajara,
            TipoUbicacion = TipoUbicacion.Almacen,
            UltimaActualizacion = DateTime.UtcNow
        });

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó", "Material", $"Material '{material.Nombre}' creado");

        return (true, "Material registrado correctamente.", MaterialResponseDto.FromEntity(material));
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
        var (uid2, uname2) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid2, uname2, "Actualizó", "Material", $"Material '{material.Nombre}' actualizado");
        return (true, "Material actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var material = await _materialesRepo.GetByIdAsync(id);
        if (material is null) return (false, "Material no encontrado.");

        await _materialesRepo.DeleteAsync(material);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó", "Material", $"Material '{material.Nombre}' eliminado");
        return (true, "Material desactivado correctamente.");
    }

    private (int UsuarioId, string NombreUsuario) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        return (id, nombre);
    }
}
