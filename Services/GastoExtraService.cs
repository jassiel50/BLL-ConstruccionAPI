using System.Security.Claims;
using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.GastosExtras;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class GastoExtraService : IGastoExtraService
{
    private readonly AppDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GastoExtraService(
        AppDbContext context,
        IBitacoraService bitacora,
        IHttpContextAccessor httpContextAccessor)
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

    private static GastoExtraDto MapToDto(GastoExtra g) => new()
    {
        Id = g.Id,
        FaseId = g.FaseId,
        Concepto = g.Concepto,
        Monto = g.Monto,
        MontoProveedor = g.MontoProveedor,
        CobradoCliente = g.CobradoCliente,
        ProveedorId = g.ProveedorId,
        NombreProveedor = g.Proveedor?.Nombre,
        Fecha = g.Fecha,
        Observaciones = g.Observaciones
    };

    public async Task<List<GastoExtraDto>> GetByFaseAsync(int faseId)
    {
        return await _context.GastosExtras
            .Include(g => g.Proveedor)
            .Where(g => g.FaseId == faseId)
            .Select(g => new GastoExtraDto
            {
                Id = g.Id,
                FaseId = g.FaseId,
                Concepto = g.Concepto,
                Monto = g.Monto,
                MontoProveedor = g.MontoProveedor,
                CobradoCliente = g.CobradoCliente,
                ProveedorId = g.ProveedorId,
                NombreProveedor = g.Proveedor != null ? g.Proveedor.Nombre : null,
                Fecha = g.Fecha,
                Observaciones = g.Observaciones
            })
            .ToListAsync();
    }

    public async Task<(bool Success, GastoExtraDto? Data)> CreateAsync(int faseId, GastoExtraRequestDto dto)
    {
        var fase = await _context.FaseProyectos
            .Include(f => f.Proyecto)
            .FirstOrDefaultAsync(f => f.Id == faseId);
        if (fase is null) return (false, null);

        var entity = new GastoExtra
        {
            FaseId = faseId,
            Concepto = dto.Concepto,
            Monto = dto.Monto,
            MontoProveedor = dto.MontoProveedor,
            CobradoCliente = dto.CobradoCliente,
            ProveedorId = dto.ProveedorId,
            Fecha = dto.Fecha,
            Observaciones = dto.Observaciones,
            FechaRegistro = DateTime.UtcNow
        };

        _context.GastosExtras.Add(entity);
        await _context.SaveChangesAsync();

        // Alerta si el gasto total se acerca a la utilidad del presupuesto
        if (fase.Proyecto is not null && fase.Proyecto.PresupuestoEstimado > 0)
        {
            var proyectoId = fase.ProyectoId;
            var faseIds = await _context.FaseProyectos
                .Where(f => f.ProyectoId == proyectoId)
                .Select(f => f.Id)
                .ToListAsync();
            var totalGastosExtras = await _context.GastosExtras
                .Where(g => faseIds.Contains(g.FaseId))
                .SumAsync(g => g.Monto);
            var gastoMateriales = (await _context.Salidas
                .Include(s => s.Detalles)
                .Where(s => s.ProyectoId == proyectoId)
                .ToListAsync())
                .SelectMany(s => s.Detalles)
                .Sum(d => d.Cantidad * d.PrecioUnitario);
            var gastoTotal = totalGastosExtras + gastoMateriales;
            var porcentaje = gastoTotal / fase.Proyecto.PresupuestoEstimado;
            if (porcentaje >= 0.85m)
                entity.Observaciones = string.IsNullOrEmpty(entity.Observaciones)
                    ? $"[ALERTA] El gasto acumulado ({porcentaje:P0}) supera el 85% del presupuesto."
                    : entity.Observaciones;
        }

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Agregó gasto extra", "GastoExtra",
            $"Gasto extra '{entity.Concepto}' (${entity.Monto:N2}) registrado en fase ID {faseId}.", ip);

        await _context.Entry(entity).Reference(e => e.Proveedor).LoadAsync();
        return (true, MapToDto(entity));
    }

    public async Task<(bool Success, GastoExtraDto? Data)> UpdateAsync(int id, GastoExtraRequestDto dto)
    {
        var entity = await _context.GastosExtras
            .Include(g => g.Proveedor)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (entity is null) return (false, null);

        entity.Concepto = dto.Concepto;
        entity.Monto = dto.Monto;
        entity.MontoProveedor = dto.MontoProveedor;
        entity.CobradoCliente = dto.CobradoCliente;
        entity.ProveedorId = dto.ProveedorId;
        entity.Fecha = dto.Fecha;
        entity.Observaciones = dto.Observaciones;

        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(e => e.Proveedor).LoadAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Actualizó gasto extra", "GastoExtra",
            $"Gasto extra '{entity.Concepto}' (ID {entity.Id}) actualizado.", ip);

        return (true, MapToDto(entity));
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.GastosExtras.FindAsync(id);
        if (entity is null) return false;

        _context.GastosExtras.Remove(entity);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó gasto extra", "GastoExtra",
            $"Gasto extra '{entity.Concepto}' (ID {entity.Id}) eliminado de fase ID {entity.FaseId}.", ip);

        return true;
    }
}
