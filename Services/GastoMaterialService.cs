using System.Security.Claims;
using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Proyectos;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class GastoMaterialService : IGastoMaterialService
{
    private readonly AppDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GastoMaterialService(AppDbContext context, IBitacoraService bitacora, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
    }

    private (int Id, string Nombre, string Ip) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "";
        return (id, nombre, ip);
    }

    private static GastoMaterialDto MapToDto(GastoMaterial g) => new()
    {
        Id = g.Id,
        ProyectoId = g.ProyectoId,
        Descripcion = g.Descripcion,
        Caracteristicas = g.Caracteristicas,
        MaterialId = g.MaterialId,
        Unidad = g.Unidad,
        Cantidad = g.Cantidad,
        CostoUnitario = g.CostoUnitario,
        Total = g.Cantidad * g.CostoUnitario,
        ProveedorId = g.ProveedorId,
        NombreProveedor = g.Proveedor?.Nombre,
        Fecha = g.Fecha,
        Observaciones = g.Observaciones,
        FechaRegistro = g.FechaRegistro
    };

    public async Task<List<GastoMaterialDto>> GetByProyectoAsync(int proyectoId)
    {
        var lista = await _context.GastosMaterial
            .AsNoTracking()
            .Include(g => g.Proveedor)
            .Where(g => g.ProyectoId == proyectoId)
            .OrderByDescending(g => g.Fecha)
            .ToListAsync();

        return lista.Select(MapToDto).ToList();
    }

    public async Task<(bool Success, string Message, GastoMaterialDto? Data)> CreateAsync(int proyectoId, GastoMaterialRequestDto dto)
    {
        var existeProyecto = await _context.Proyectos.AnyAsync(p => p.Id == proyectoId);
        if (!existeProyecto) return (false, "Proyecto no encontrado.", null);

        if (string.IsNullOrWhiteSpace(dto.Descripcion))
            return (false, "Captura la descripción del material.", null);

        if (dto.Cantidad <= 0)
            return (false, "La cantidad debe ser mayor a cero.", null);

        var (uid, uname, ip) = GetUsuarioInfo();

        var entity = new GastoMaterial
        {
            ProyectoId = proyectoId,
            Descripcion = dto.Descripcion.Trim(),
            Caracteristicas = dto.Caracteristicas,
            MaterialId = dto.MaterialId,
            Unidad = dto.Unidad.Trim(),
            Cantidad = dto.Cantidad,
            CostoUnitario = dto.CostoUnitario,
            ProveedorId = dto.ProveedorId,
            Fecha = dto.Fecha,
            Observaciones = dto.Observaciones,
            CreadoPorId = uid,
            FechaRegistro = DateTime.UtcNow
        };

        _context.GastosMaterial.Add(entity);
        await _context.SaveChangesAsync();

        if (entity.ProveedorId.HasValue)
            await _context.Entry(entity).Reference(e => e.Proveedor).LoadAsync();

        await _bitacora.RegistrarAsync(uid, uname, "Registró gasto de material", "GastoMaterial",
            $"Gasto de material '{entity.Descripcion}' (${entity.Cantidad * entity.CostoUnitario:N2}) registrado en proyecto ID {proyectoId}.", ip);

        return (true, "Gasto de material registrado.", MapToDto(entity));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var entity = await _context.GastosMaterial.FirstOrDefaultAsync(g => g.Id == id);
        if (entity is null) return (false, "Gasto de material no encontrado.");

        _context.GastosMaterial.Remove(entity);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó gasto de material", "GastoMaterial",
            $"Gasto de material '{entity.Descripcion}' (ID {id}) eliminado del proyecto ID {entity.ProyectoId}.", ip);

        return (true, "Gasto de material eliminado.");
    }
}
