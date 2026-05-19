using BLL_ConstruccionAPI.DTOs.Catalogos;
using BLL_ConstruccionAPI.Models.Inventario.Cátalogos;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BLL_ConstruccionAPI.Services;

public class CatalogosService : ICatalogosService
{
    private readonly ICatalogosRepository _catalogosRepo;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CatalogosService(
        ICatalogosRepository catalogosRepo,
        IBitacoraService bitacora,
        IHttpContextAccessor httpContextAccessor)
    {
        _catalogosRepo = catalogosRepo;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
    }

    // ─── CategoriaProveedor ───────────────────────────────────────────────────
    public async Task<IEnumerable<CategoriaProveedorResponseDto>> GetAllCategoriasProveedorAsync()
    {
        var categorias = await _catalogosRepo.GetAllCategoriasProveedorAsync();
        return categorias.Select(CategoriaProveedorResponseDto.FromEntity);
    }

    public async Task<CategoriaProveedorResponseDto?> GetCategoriaProveedorByIdAsync(int id)
    {
        var categoria = await _catalogosRepo.GetCategoriaProveedorByIdAsync(id);
        return categoria is null ? null : CategoriaProveedorResponseDto.FromEntity(categoria);
    }

    public async Task<(bool Success, string Message, CategoriaProveedorResponseDto? Data)> CreateCategoriaProveedorAsync(CategoriaProveedorRequestDto dto)
    {
        if (await _catalogosRepo.ExisteCategoriaProveedorAsync(dto.Nombre))
            return (false, "Ya existe una categoría de proveedor con ese nombre.", null);

        var categoria = new CategoriaProveedor { Nombre = dto.Nombre, Descripcion = dto.Descripcion, Activo = true };
        await _catalogosRepo.CreateCategoriaProveedorAsync(categoria);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó", "CategoríaProveedor", $"Categoría de proveedor '{categoria.Nombre}' creada");
        return (true, "Categoría de proveedor creada correctamente.", CategoriaProveedorResponseDto.FromEntity(categoria));
    }

    public async Task<(bool Success, string Message)> UpdateCategoriaProveedorAsync(int id, CategoriaProveedorRequestDto dto)
    {
        var categoria = await _catalogosRepo.GetCategoriaProveedorByIdAsync(id);
        if (categoria is null) return (false, "Categoría de proveedor no encontrada.");

        if (categoria.Nombre != dto.Nombre && await _catalogosRepo.ExisteCategoriaProveedorAsync(dto.Nombre))
            return (false, "Ya existe una categoría de proveedor con ese nombre.");

        categoria.Nombre = dto.Nombre;
        categoria.Descripcion = dto.Descripcion;
        await _catalogosRepo.UpdateCategoriaProveedorAsync(categoria);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Actualizó", "CategoríaProveedor", $"Categoría de proveedor '{categoria.Nombre}' actualizada");
        return (true, "Categoría de proveedor actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteCategoriaProveedorAsync(int id)
    {
        var categoria = await _catalogosRepo.GetCategoriaProveedorByIdAsync(id);
        if (categoria is null) return (false, "Categoría de proveedor no encontrada.");

        await _catalogosRepo.DeleteCategoriaProveedorAsync(categoria);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó", "CategoríaProveedor", $"Categoría de proveedor '{categoria.Nombre}' desactivada");
        return (true, "Categoría de proveedor desactivada correctamente.");
    }

    // ─── CategoriaCliente ─────────────────────────────────────────────────────
    public async Task<IEnumerable<CategoriaClienteResponseDto>> GetAllCategoriasClienteAsync()
    {
        var categorias = await _catalogosRepo.GetAllCategoriasClienteAsync();
        return categorias.Select(CategoriaClienteResponseDto.FromEntity);
    }

    public async Task<CategoriaClienteResponseDto?> GetCategoriaClienteByIdAsync(int id)
    {
        var categoria = await _catalogosRepo.GetCategoriaClienteByIdAsync(id);
        return categoria is null ? null : CategoriaClienteResponseDto.FromEntity(categoria);
    }

    public async Task<(bool Success, string Message, CategoriaClienteResponseDto? Data)> CreateCategoriaClienteAsync(CategoriaClienteRequestDto dto)
    {
        if (await _catalogosRepo.ExisteCategoriaClienteAsync(dto.Nombre))
            return (false, "Ya existe una categoría de cliente con ese nombre.", null);

        var categoria = new CategoriaCliente { Nombre = dto.Nombre, Descripcion = dto.Descripcion, Activo = true };
        await _catalogosRepo.CreateCategoriaClienteAsync(categoria);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó", "CategoríaCliente", $"Categoría de cliente '{categoria.Nombre}' creada");
        return (true, "Categoría de cliente creada correctamente.", CategoriaClienteResponseDto.FromEntity(categoria));
    }

    public async Task<(bool Success, string Message)> UpdateCategoriaClienteAsync(int id, CategoriaClienteRequestDto dto)
    {
        var categoria = await _catalogosRepo.GetCategoriaClienteByIdAsync(id);
        if (categoria is null) return (false, "Categoría de cliente no encontrada.");

        if (categoria.Nombre != dto.Nombre && await _catalogosRepo.ExisteCategoriaClienteAsync(dto.Nombre))
            return (false, "Ya existe una categoría de cliente con ese nombre.");

        categoria.Nombre = dto.Nombre;
        categoria.Descripcion = dto.Descripcion;
        await _catalogosRepo.UpdateCategoriaClienteAsync(categoria);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Actualizó", "CategoríaCliente", $"Categoría de cliente '{categoria.Nombre}' actualizada");
        return (true, "Categoría de cliente actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteCategoriaClienteAsync(int id)
    {
        var categoria = await _catalogosRepo.GetCategoriaClienteByIdAsync(id);
        if (categoria is null) return (false, "Categoría de cliente no encontrada.");

        await _catalogosRepo.DeleteCategoriaClienteAsync(categoria);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó", "CategoríaCliente", $"Categoría de cliente '{categoria.Nombre}' desactivada");
        return (true, "Categoría de cliente desactivada correctamente.");
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
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó", "CategoríaMaterial", $"Categoría de material '{categoria.Nombre}' creada");
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
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Actualizó", "CategoríaMaterial", $"Categoría de material '{categoria.Nombre}' actualizada");
        return (true, "Categoría actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteCategoriaAsync(int id)
    {
        var categoria = await _catalogosRepo.GetCategoriaByIdAsync(id);
        if (categoria is null) return (false, "Categoría no encontrada.");

        await _catalogosRepo.DeleteCategoriaAsync(categoria);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó", "CategoríaMaterial", $"Categoría de material '{categoria.Nombre}' desactivada");
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
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó", "CategoríaHerramienta", $"Categoría de herramienta '{categoria.Nombre}' creada");
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
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Actualizó", "CategoríaHerramienta", $"Categoría de herramienta '{categoria.Nombre}' actualizada");
        return (true, "Categoría de herramienta actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteCategoriaHerramientaAsync(int id)
    {
        var categoria = await _catalogosRepo.GetCategoriaHerramientaByIdAsync(id);
        if (categoria is null) return (false, "Categoría de herramienta no encontrada.");

        await _catalogosRepo.DeleteCategoriaHerramientaAsync(categoria);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó", "CategoríaHerramienta", $"Categoría de herramienta '{categoria.Nombre}' desactivada");
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
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó", "UnidadMedida", $"Unidad de medida '{unidad.Nombre} ({unidad.Abreviatura})' creada");
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
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Actualizó", "UnidadMedida", $"Unidad de medida '{unidad.Nombre}' actualizada");
        return (true, "Unidad de medida actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteUnidadAsync(int id)
    {
        var unidad = await _catalogosRepo.GetUnidadByIdAsync(id);
        if (unidad is null) return (false, "Unidad de medida no encontrada.");

        await _catalogosRepo.DeleteUnidadAsync(unidad);
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó", "UnidadMedida", $"Unidad de medida '{unidad.Nombre}' desactivada");
        return (true, "Unidad de medida desactivada correctamente.");
    }

    private (int UsuarioId, string NombreUsuario) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        return (id, nombre);
    }
}
