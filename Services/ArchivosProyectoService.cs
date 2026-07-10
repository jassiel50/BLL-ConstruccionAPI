using System.Security.Claims;
using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Archivos;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class ArchivosProyectoService : IArchivosProyectoService
{
    private readonly AppDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly long MaxTamanioBytes = 20 * 1024 * 1024; // 20 MB

    public ArchivosProyectoService(AppDbContext context, IBitacoraService bitacora, IHttpContextAccessor httpContextAccessor)
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

    private static ArchivoProyectoDto MapToDto(ArchivoProyecto a, string? carpetaNombre = null) => new()
    {
        Id = a.Id,
        ProyectoId = a.ProyectoId,
        NombreOriginal = a.NombreOriginal,
        TipoDocumento = a.TipoDocumento.ToString(),
        ContentType = a.ContentType,
        TamanioBytes = a.TamanioBytes,
        SubidoPorId = a.SubidoPorId,
        FechaSubida = a.FechaSubida,
        CarpetaId = a.CarpetaId,
        CarpetaNombre = a.Carpeta?.Nombre ?? carpetaNombre
    };

    public async Task<List<ArchivoProyectoDto>> GetByProyectoAsync(int proyectoId) =>
        await _context.ArchivosProyecto
            .Include(a => a.Carpeta)
            .Where(a => a.ProyectoId == proyectoId)
            .OrderByDescending(a => a.FechaSubida)
            .Select(a => new ArchivoProyectoDto
            {
                Id = a.Id, ProyectoId = a.ProyectoId, NombreOriginal = a.NombreOriginal,
                TipoDocumento = a.TipoDocumento.ToString(), ContentType = a.ContentType,
                TamanioBytes = a.TamanioBytes, SubidoPorId = a.SubidoPorId, FechaSubida = a.FechaSubida,
                CarpetaId = a.CarpetaId, CarpetaNombre = a.Carpeta != null ? a.Carpeta.Nombre : null
            })
            .ToListAsync();

    public async Task<(bool Success, string Message, ArchivoProyectoDto? Data)> SubirAsync(int proyectoId, IFormFile archivo, string tipoDocumento, int? carpetaId)
    {
        var proyectoExists = await _context.Proyectos.AnyAsync(p => p.Id == proyectoId && p.Activo);
        if (!proyectoExists) return (false, "Proyecto no encontrado.", null);

        if (archivo.Length == 0) return (false, "El archivo está vacío.", null);
        if (archivo.Length > MaxTamanioBytes) return (false, "El archivo supera el límite de 20 MB.", null);

        if (!Enum.TryParse<TipoDocumentoProyecto>(tipoDocumento, out var tipo))
            return (false, $"TipoDocumento inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<TipoDocumentoProyecto>())}.", null);

        CarpetaProyecto? carpeta = null;
        if (carpetaId.HasValue)
        {
            carpeta = await _context.CarpetasProyecto.FindAsync(carpetaId.Value);
            if (carpeta is null || carpeta.ProyectoId != proyectoId)
                return (false, "La carpeta no existe o no pertenece a este proyecto.", null);
        }

        using var ms = new MemoryStream();
        await archivo.CopyToAsync(ms);

        var (uid, uname, ip) = GetUsuarioInfo();
        var entity = new ArchivoProyecto
        {
            ProyectoId = proyectoId,
            NombreOriginal = archivo.FileName,
            TipoDocumento = tipo,
            ContentType = archivo.ContentType,
            TamanioBytes = archivo.Length,
            Contenido = ms.ToArray(),
            SubidoPorId = uid,
            FechaSubida = DateTime.UtcNow,
            CarpetaId = carpeta?.Id
        };

        _context.ArchivosProyecto.Add(entity);
        await _context.SaveChangesAsync();

        await _bitacora.RegistrarAsync(uid, uname, "Subió archivo", "ArchivoProyecto",
            $"Archivo '{entity.NombreOriginal}' ({tipo}) subido al proyecto ID {proyectoId}" +
            (carpeta is not null ? $" en la carpeta '{carpeta.Nombre}'." : "."), ip);

        return (true, "Archivo subido correctamente.", MapToDto(entity, carpeta?.Nombre));
    }

    public async Task<(bool Found, string NombreOriginal, string ContentType, byte[]? Contenido)> DescargarAsync(int id)
    {
        var archivo = await _context.ArchivosProyecto.FindAsync(id);
        if (archivo is null) return (false, "", "", null);
        return (true, archivo.NombreOriginal, archivo.ContentType, archivo.Contenido);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var archivo = await _context.ArchivosProyecto.FindAsync(id);
        if (archivo is null) return (false, "Archivo no encontrado.");

        _context.ArchivosProyecto.Remove(archivo);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó archivo", "ArchivoProyecto",
            $"Archivo '{archivo.NombreOriginal}' (ID {archivo.Id}) eliminado.", ip);

        return (true, "Archivo eliminado.");
    }

    public async Task<List<CarpetaProyectoDto>> GetCarpetasAsync(int proyectoId)
    {
        var carpetas = await _context.CarpetasProyecto
            .Where(c => c.ProyectoId == proyectoId)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        var conteos = await _context.ArchivosProyecto
            .Where(a => a.ProyectoId == proyectoId && a.CarpetaId != null)
            .GroupBy(a => a.CarpetaId)
            .Select(g => new { CarpetaId = g.Key!.Value, Total = g.Count() })
            .ToDictionaryAsync(g => g.CarpetaId, g => g.Total);

        return carpetas.Select(c => new CarpetaProyectoDto
        {
            Id = c.Id,
            ProyectoId = c.ProyectoId,
            Nombre = c.Nombre,
            FechaCreacion = c.FechaCreacion,
            NumeroArchivos = conteos.TryGetValue(c.Id, out var total) ? total : 0
        }).ToList();
    }

    public async Task<(bool Success, string Message, CarpetaProyectoDto? Data)> CrearCarpetaAsync(int proyectoId, CarpetaProyectoRequestDto dto)
    {
        var proyectoExists = await _context.Proyectos.AnyAsync(p => p.Id == proyectoId && p.Activo);
        if (!proyectoExists) return (false, "Proyecto no encontrado.", null);

        var nombre = dto.Nombre?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "El nombre de la carpeta es requerido.", null);

        var yaExiste = await _context.CarpetasProyecto
            .AnyAsync(c => c.ProyectoId == proyectoId && c.Nombre.ToLower() == nombre.ToLower());
        if (yaExiste)
            return (false, "Ya existe una carpeta con ese nombre en este proyecto.", null);

        var (uid, uname, ip) = GetUsuarioInfo();
        var entity = new CarpetaProyecto
        {
            ProyectoId = proyectoId,
            Nombre = nombre,
            CreadoPorId = uid,
            FechaCreacion = DateTime.UtcNow
        };

        _context.CarpetasProyecto.Add(entity);
        await _context.SaveChangesAsync();

        await _bitacora.RegistrarAsync(uid, uname, "Creó carpeta", "CarpetaProyecto",
            $"Carpeta '{entity.Nombre}' creada en el proyecto ID {proyectoId}.", ip);

        return (true, "Carpeta creada correctamente.", new CarpetaProyectoDto
        {
            Id = entity.Id,
            ProyectoId = entity.ProyectoId,
            Nombre = entity.Nombre,
            FechaCreacion = entity.FechaCreacion,
            NumeroArchivos = 0
        });
    }

    public async Task<(bool Success, string Message)> EliminarCarpetaAsync(int carpetaId)
    {
        var carpeta = await _context.CarpetasProyecto.FindAsync(carpetaId);
        if (carpeta is null) return (false, "Carpeta no encontrada.");

        await _context.ArchivosProyecto
            .Where(a => a.CarpetaId == carpetaId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.CarpetaId, (int?)null));

        _context.CarpetasProyecto.Remove(carpeta);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó carpeta", "CarpetaProyecto",
            $"Carpeta '{carpeta.Nombre}' (ID {carpeta.Id}) eliminada del proyecto ID {carpeta.ProyectoId}. Sus archivos se movieron a la raíz.", ip);

        return (true, "Carpeta eliminada. Los archivos que contenía se movieron a la raíz del proyecto.");
    }
}
