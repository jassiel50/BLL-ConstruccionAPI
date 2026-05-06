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

    public async Task<List<GastoExtraDto>> GetByFaseAsync(int faseId)
    {
        return await _context.GastosExtras
            .Where(g => g.FaseId == faseId)
            .Select(g => new GastoExtraDto
            {
                Id = g.Id,
                FaseId = g.FaseId,
                Concepto = g.Concepto,
                Monto = g.Monto,
                Fecha = g.Fecha,
                Observaciones = g.Observaciones
            })
            .ToListAsync();
    }

    public async Task<(bool Success, GastoExtraDto? Data)> CreateAsync(int faseId, GastoExtraRequestDto dto)
    {
        var faseExists = await _context.FaseProyectos.AnyAsync(f => f.Id == faseId);
        if (!faseExists)
            return (false, null);

        var entity = new GastoExtra
        {
            FaseId = faseId,
            Concepto = dto.Concepto,
            Monto = dto.Monto,
            Fecha = dto.Fecha,
            Observaciones = dto.Observaciones,
            FechaRegistro = DateTime.UtcNow
        };

        _context.GastosExtras.Add(entity);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Agregó gasto extra", "GastoExtra",
            $"Gasto extra '{entity.Concepto}' (${entity.Monto:N2}) registrado en fase ID {faseId}.", ip);

        return (true, new GastoExtraDto
        {
            Id = entity.Id,
            FaseId = entity.FaseId,
            Concepto = entity.Concepto,
            Monto = entity.Monto,
            Fecha = entity.Fecha,
            Observaciones = entity.Observaciones
        });
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
