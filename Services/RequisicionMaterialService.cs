using System.Security.Claims;
using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Proyectos;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Reports;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace BLL_ConstruccionAPI.Services;

public class RequisicionMaterialService : IRequisicionMaterialService
{
    private readonly AppDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequisicionMaterialService(AppDbContext context, IBitacoraService bitacora, IHttpContextAccessor httpContextAccessor)
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

    private static RequisicionMaterialDto MapToDto(RequisicionMaterial r) => new()
    {
        Id = r.Id,
        ProyectoId = r.ProyectoId,
        ProyectoNombre = r.Proyecto?.Nombre ?? string.Empty,
        Folio = r.Folio,
        SeRequierePara = r.SeRequierePara,
        SeSuministraPor = r.SeSuministraPor,
        FechaSolicitud = r.FechaSolicitud,
        SolicitoNombre = r.SolicitoNombre,
        FechaCreacion = r.FechaCreacion,
        Detalles = r.Detalles
            .OrderBy(d => d.Orden)
            .Select(d => new RequisicionMaterialDetalleDto
            {
                Id = d.Id,
                Orden = d.Orden,
                Descripcion = d.Descripcion,
                Unidad = d.Unidad,
                Cantidad = d.Cantidad,
                AreaComentarios = d.AreaComentarios,
                Status = d.Status,
                MaterialId = d.MaterialId
            }).ToList()
    };

    public async Task<List<RequisicionMaterialDto>> GetByProyectoAsync(int proyectoId)
    {
        var lista = await _context.RequisicionesMaterial
            .AsNoTracking()
            .Include(r => r.Proyecto)
            .Include(r => r.Detalles)
            .Where(r => r.ProyectoId == proyectoId)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();

        return lista.Select(MapToDto).ToList();
    }

    public async Task<RequisicionMaterialDto?> GetByIdAsync(int id)
    {
        var entity = await _context.RequisicionesMaterial
            .AsNoTracking()
            .Include(r => r.Proyecto)
            .Include(r => r.Detalles)
            .FirstOrDefaultAsync(r => r.Id == id);

        return entity is null ? null : MapToDto(entity);
    }

    public async Task<(bool Success, string Message, RequisicionMaterialDto? Data)> CreateAsync(int proyectoId, RequisicionMaterialRequestDto dto)
    {
        var proyecto = await _context.Proyectos.FirstOrDefaultAsync(p => p.Id == proyectoId);
        if (proyecto is null) return (false, "Proyecto no encontrado.", null);

        if (dto.Detalles.Count == 0)
            return (false, "Agrega al menos un renglón a la requisición.", null);

        if (dto.Detalles.Any(d => string.IsNullOrWhiteSpace(d.Descripcion)))
            return (false, "Todos los renglones deben tener descripción.", null);

        var (uid, uname, ip) = GetUsuarioInfo();

        var siguienteFolio = await _context.RequisicionesMaterial
            .Where(r => r.ProyectoId == proyectoId)
            .Select(r => (int?)r.Folio)
            .MaxAsync() ?? 0;

        var entity = new RequisicionMaterial
        {
            ProyectoId = proyectoId,
            Folio = siguienteFolio + 1,
            SeRequierePara = dto.SeRequierePara.Trim(),
            SeSuministraPor = dto.SeSuministraPor.Trim(),
            FechaSolicitud = dto.FechaSolicitud,
            SolicitoNombre = string.IsNullOrWhiteSpace(dto.SolicitoNombre) ? uname : dto.SolicitoNombre.Trim(),
            CreadoPorId = uid,
            FechaCreacion = DateTime.UtcNow,
            Detalles = dto.Detalles.Select((d, i) => new RequisicionMaterialDetalle
            {
                Orden = i + 1,
                Descripcion = d.Descripcion.Trim(),
                Unidad = d.Unidad.Trim(),
                Cantidad = d.Cantidad,
                AreaComentarios = d.AreaComentarios,
                Status = string.IsNullOrWhiteSpace(d.Status) ? "Pendiente" : d.Status.Trim(),
                MaterialId = d.MaterialId
            }).ToList()
        };

        _context.RequisicionesMaterial.Add(entity);
        await _context.SaveChangesAsync();

        await _bitacora.RegistrarAsync(uid, uname, "Creó requisición de materiales", "RequisicionMaterial",
            $"Requisición #{entity.Folio} ({entity.Detalles.Count} renglón(es)) creada en proyecto '{proyecto.Nombre}' (ID {proyectoId}).", ip);

        entity.Proyecto = proyecto;
        return (true, "Requisición creada.", MapToDto(entity));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var entity = await _context.RequisicionesMaterial.FirstOrDefaultAsync(r => r.Id == id);
        if (entity is null) return (false, "Requisición no encontrada.");

        _context.RequisicionesMaterial.Remove(entity);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó requisición de materiales", "RequisicionMaterial",
            $"Requisición #{entity.Folio} (ID {id}) eliminada.", ip);

        return (true, "Requisición eliminada.");
    }

    public async Task<(bool Success, string Message, byte[]? Pdf)> GenerarPdfAsync(int id)
    {
        var entity = await _context.RequisicionesMaterial
            .AsNoTracking()
            .Include(r => r.Proyecto)
            .Include(r => r.Detalles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (entity is null) return (false, "Requisición no encontrada.", null);

        var documento = new RequisicionMaterialDocument(entity);
        var pdf = documento.GeneratePdf();
        return (true, "OK", pdf);
    }
}
