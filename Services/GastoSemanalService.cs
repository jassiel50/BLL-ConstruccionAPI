using System.Security.Claims;
using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.GastosSemanales;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class GastoSemanalService : IGastoSemanalService
{
    private readonly AppDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GastoSemanalService(AppDbContext context, IBitacoraService bitacora, IHttpContextAccessor httpContextAccessor)
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

    private static GastoSemanalDto MapToDto(GastoSemanal g, int diasDesdeUltimo = 0) => new()
    {
        Id = g.Id,
        ProyectoId = g.ProyectoId,
        Concepto = g.Concepto,
        Monto = g.Monto,
        FechaInicio = g.FechaInicio,
        FechaFin = g.FechaFin,
        Tipo = g.Tipo,
        Observaciones = g.Observaciones,
        FechaRegistro = g.FechaRegistro,
        DiasDesdeUltimo = diasDesdeUltimo
    };

    public async Task<List<GastoSemanalDto>> GetByProyectoAsync(int proyectoId)
    {
        var lista = await _context.GastosSemanales
            .Where(g => g.ProyectoId == proyectoId)
            .OrderByDescending(g => g.FechaRegistro)
            .ToListAsync();

        var hoy = DateTime.UtcNow.Date;
        return lista.Select(g => MapToDto(g, (int)(hoy - g.FechaFin.Date).TotalDays)).ToList();
    }

    public async Task<(bool Success, GastoSemanalDto? Data)> CreateAsync(int proyectoId, GastoSemanalRequestDto dto)
    {
        var existe = await _context.Proyectos.AnyAsync(p => p.Id == proyectoId);
        if (!existe) return (false, null);

        var entity = new GastoSemanal
        {
            ProyectoId = proyectoId,
            Concepto = dto.Concepto,
            Monto = dto.Monto,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            Tipo = dto.Tipo,
            Observaciones = dto.Observaciones,
            FechaRegistro = DateTime.UtcNow
        };

        _context.GastosSemanales.Add(entity);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Registró gasto semanal", "GastoSemanal",
            $"Gasto semanal '{entity.Concepto}' (${entity.Monto:N2}) registrado en proyecto ID {proyectoId}.", ip);

        var dias = (int)(DateTime.UtcNow.Date - entity.FechaFin.Date).TotalDays;
        return (true, MapToDto(entity, dias));
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.GastosSemanales.FindAsync(id);
        if (entity is null) return false;

        _context.GastosSemanales.Remove(entity);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó gasto semanal", "GastoSemanal",
            $"Gasto semanal ID {entity.Id} eliminado del proyecto ID {entity.ProyectoId}.", ip);

        return true;
    }

    public async Task<(bool Found, GastoSemanalDto? Data)> GetUltimoAsync(int proyectoId)
    {
        var ultimo = await _context.GastosSemanales
            .Where(g => g.ProyectoId == proyectoId)
            .OrderByDescending(g => g.FechaFin)
            .FirstOrDefaultAsync();

        if (ultimo is null) return (false, null);

        var dias = (int)(DateTime.UtcNow.Date - ultimo.FechaFin.Date).TotalDays;
        return (true, MapToDto(ultimo, dias));
    }
}
