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

    private static ArchivoProyectoDto MapToDto(ArchivoProyecto a) => new()
    {
        Id = a.Id,
        ProyectoId = a.ProyectoId,
        NombreOriginal = a.NombreOriginal,
        TipoDocumento = a.TipoDocumento.ToString(),
        ContentType = a.ContentType,
        TamanioBytes = a.TamanioBytes,
        SubidoPorId = a.SubidoPorId,
        FechaSubida = a.FechaSubida
    };

    public async Task<List<ArchivoProyectoDto>> GetByProyectoAsync(int proyectoId) =>
        await _context.ArchivosProyecto
            .Where(a => a.ProyectoId == proyectoId)
            .OrderByDescending(a => a.FechaSubida)
            .Select(a => new ArchivoProyectoDto
            {
                Id = a.Id, ProyectoId = a.ProyectoId, NombreOriginal = a.NombreOriginal,
                TipoDocumento = a.TipoDocumento.ToString(), ContentType = a.ContentType,
                TamanioBytes = a.TamanioBytes, SubidoPorId = a.SubidoPorId, FechaSubida = a.FechaSubida
            })
            .ToListAsync();

    public async Task<(bool Success, string Message, ArchivoProyectoDto? Data)> SubirAsync(int proyectoId, IFormFile archivo, string tipoDocumento)
    {
        var proyectoExists = await _context.Proyectos.AnyAsync(p => p.Id == proyectoId && p.Activo);
        if (!proyectoExists) return (false, "Proyecto no encontrado.", null);

        if (archivo.Length == 0) return (false, "El archivo está vacío.", null);
        if (archivo.Length > MaxTamanioBytes) return (false, "El archivo supera el límite de 20 MB.", null);

        if (!Enum.TryParse<TipoDocumentoProyecto>(tipoDocumento, out var tipo))
            return (false, $"TipoDocumento inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<TipoDocumentoProyecto>())}.", null);

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
            FechaSubida = DateTime.UtcNow
        };

        _context.ArchivosProyecto.Add(entity);
        await _context.SaveChangesAsync();

        await _bitacora.RegistrarAsync(uid, uname, "Subió archivo", "ArchivoProyecto",
            $"Archivo '{entity.NombreOriginal}' ({tipo}) subido al proyecto ID {proyectoId}.", ip);

        return (true, "Archivo subido correctamente.", MapToDto(entity));
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
}
