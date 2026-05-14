using System.Security.Claims;
using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Checklist;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class ChecklistService : IChecklistService
{
    private readonly AppDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChecklistService(AppDbContext context, IBitacoraService bitacora, IHttpContextAccessor httpContextAccessor)
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

    private static ChecklistItemDto MapToDto(ChecklistItem c) => new()
    {
        Id = c.Id,
        ProyectoId = c.ProyectoId,
        FaseId = c.FaseId,
        Titulo = c.Titulo,
        Completado = c.Completado,
        FechaCompletado = c.FechaCompletado,
        FechaRegistro = c.FechaRegistro
    };

    public async Task<List<ChecklistItemDto>> GetByProyectoAsync(int proyectoId) =>
        await _context.ChecklistItems
            .Where(c => c.ProyectoId == proyectoId)
            .OrderBy(c => c.FechaRegistro)
            .Select(c => new ChecklistItemDto
            {
                Id = c.Id, ProyectoId = c.ProyectoId, FaseId = c.FaseId,
                Titulo = c.Titulo, Completado = c.Completado,
                FechaCompletado = c.FechaCompletado, FechaRegistro = c.FechaRegistro
            })
            .ToListAsync();

    public async Task<List<ChecklistItemDto>> GetByFaseAsync(int faseId) =>
        await _context.ChecklistItems
            .Where(c => c.FaseId == faseId)
            .OrderBy(c => c.FechaRegistro)
            .Select(c => new ChecklistItemDto
            {
                Id = c.Id, ProyectoId = c.ProyectoId, FaseId = c.FaseId,
                Titulo = c.Titulo, Completado = c.Completado,
                FechaCompletado = c.FechaCompletado, FechaRegistro = c.FechaRegistro
            })
            .ToListAsync();

    public async Task<(bool Success, string Message, ChecklistItemDto? Data)> CreateAsync(int proyectoId, ChecklistItemRequestDto dto)
    {
        var proyectoExists = await _context.Proyectos.AnyAsync(p => p.Id == proyectoId && p.Activo);
        if (!proyectoExists) return (false, "Proyecto no encontrado.", null);

        if (dto.FaseId.HasValue)
        {
            var faseExists = await _context.FaseProyectos.AnyAsync(f => f.Id == dto.FaseId && f.ProyectoId == proyectoId);
            if (!faseExists) return (false, "Fase no encontrada en este proyecto.", null);
        }

        var item = new ChecklistItem
        {
            ProyectoId = proyectoId,
            FaseId = dto.FaseId,
            Titulo = dto.Titulo,
            Completado = false,
            FechaRegistro = DateTime.UtcNow
        };

        _context.ChecklistItems.Add(item);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó checklist item", "ChecklistItem",
            $"Item '{item.Titulo}' creado en proyecto ID {proyectoId}.", ip);

        return (true, "Item creado correctamente.", MapToDto(item));
    }

    public async Task<(bool Success, string Message)> CompletarAsync(int id)
    {
        var item = await _context.ChecklistItems.FindAsync(id);
        if (item is null) return (false, "Item no encontrado.");
        if (item.Completado) return (false, "El item ya está completado.");

        var (uid, uname, ip) = GetUsuarioInfo();
        item.Completado = true;
        item.FechaCompletado = DateTime.UtcNow;
        item.CompletadoPorId = uid;

        await _context.SaveChangesAsync();
        await _bitacora.RegistrarAsync(uid, uname, "Completó checklist item", "ChecklistItem",
            $"Item '{item.Titulo}' (ID {item.Id}) completado.", ip);

        return (true, "Item completado.");
    }

    public async Task<(bool Success, string Message)> DescompletarAsync(int id)
    {
        var item = await _context.ChecklistItems.FindAsync(id);
        if (item is null) return (false, "Item no encontrado.");
        if (!item.Completado) return (false, "El item no está completado.");

        item.Completado = false;
        item.FechaCompletado = null;
        item.CompletadoPorId = null;

        await _context.SaveChangesAsync();
        return (true, "Item regresado a pendiente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var item = await _context.ChecklistItems.FindAsync(id);
        if (item is null) return (false, "Item no encontrado.");

        _context.ChecklistItems.Remove(item);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó checklist item", "ChecklistItem",
            $"Item '{item.Titulo}' (ID {item.Id}) eliminado.", ip);

        return (true, "Item eliminado.");
    }
}
