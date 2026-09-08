using System.Security.Claims;
using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Reportes;
using BLL_ConstruccionAPI.Models.Reportes;
using BLL_ConstruccionAPI.Reports;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace BLL_ConstruccionAPI.Services;

public class ReporteEntregaConformidadService : IReporteEntregaConformidadService
{
    private static readonly long MaxTamanioBytes = 20 * 1024 * 1024; // 20 MB

    private readonly AppDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ReporteEntregaConformidadService(AppDbContext context, IBitacoraService bitacora, IHttpContextAccessor httpContextAccessor)
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

    private static ReporteEntregaConformidadDto MapToDto(ReporteEntregaConformidad r) => new()
    {
        Id = r.Id,
        Folio = r.Folio,
        Fecha = r.Fecha,
        ProyectoId = r.ProyectoId,
        ProyectoNombreLibre = r.ProyectoNombreLibre,
        ProyectoNombre = r.Proyecto?.Nombre ?? r.ProyectoNombreLibre ?? string.Empty,
        ClienteId = r.ClienteId,
        EmpresaNombreLibre = r.EmpresaNombreLibre,
        EmpresaNombre = r.Cliente?.Nombre ?? r.EmpresaNombreLibre ?? string.Empty,
        ContactoNombre = r.ContactoNombre,
        Concepto = r.Concepto,
        OrdenCompra = r.OrdenCompra,
        Descripcion = r.Descripcion,
        CondicionesEntrega = r.CondicionesEntrega,
        EntregaNombre = r.EntregaNombre,
        EntregaEmpresa = r.EntregaEmpresa,
        RecibeNombre = r.RecibeNombre,
        RecibeEmpresa = r.RecibeEmpresa,
        TieneDocumentoFirmado = r.DocumentoFirmadoContenido is { Length: > 0 },
        DocumentoFirmadoFecha = r.DocumentoFirmadoFecha,
        FechaCreacion = r.FechaCreacion,
        Fotos = r.Fotos.Select(f => new ReporteEntregaConformidadFotoDto
        {
            Id = f.Id,
            NombreOriginal = f.NombreOriginal,
            ContentType = f.ContentType
        }).ToList()
    };

    public async Task<List<ReporteEntregaConformidadDto>> GetAllAsync(int? proyectoId = null)
    {
        var query = _context.ReportesEntregaConformidad
            .AsNoTracking()
            .Include(r => r.Proyecto)
            .Include(r => r.Cliente)
            .Include(r => r.Fotos)
            .AsQueryable();

        if (proyectoId.HasValue)
            query = query.Where(r => r.ProyectoId == proyectoId.Value);

        var lista = await query.OrderByDescending(r => r.FechaCreacion).ToListAsync();
        return lista.Select(MapToDto).ToList();
    }

    public async Task<ReporteEntregaConformidadDto?> GetByIdAsync(int id)
    {
        var entity = await _context.ReportesEntregaConformidad
            .AsNoTracking()
            .Include(r => r.Proyecto)
            .Include(r => r.Cliente)
            .Include(r => r.Fotos)
            .FirstOrDefaultAsync(r => r.Id == id);

        return entity is null ? null : MapToDto(entity);
    }

    public async Task<(bool Success, string Message, ReporteEntregaConformidadDto? Data)> CreateAsync(ReporteEntregaConformidadRequestDto dto, int usuarioId)
    {
        if (string.IsNullOrWhiteSpace(dto.Concepto))
            return (false, "Captura el concepto de la entrega.", null);

        if (dto.ClienteId is null && string.IsNullOrWhiteSpace(dto.EmpresaNombreLibre))
            return (false, "Selecciona un cliente del catálogo o captura el nombre de la empresa.", null);

        var (uid, uname, ip) = GetUsuarioInfo();

        var siguienteFolio = await _context.ReportesEntregaConformidad
            .Select(r => (int?)r.Folio)
            .MaxAsync() ?? 0;

        var entity = new ReporteEntregaConformidad
        {
            Folio = siguienteFolio + 1,
            Fecha = dto.Fecha,
            ProyectoId = dto.ProyectoId,
            ProyectoNombreLibre = dto.ProyectoNombreLibre,
            ClienteId = dto.ClienteId,
            EmpresaNombreLibre = dto.EmpresaNombreLibre,
            ContactoNombre = dto.ContactoNombre,
            Concepto = dto.Concepto.Trim(),
            OrdenCompra = dto.OrdenCompra,
            Descripcion = dto.Descripcion,
            CondicionesEntrega = dto.CondicionesEntrega,
            EntregaNombre = string.IsNullOrWhiteSpace(dto.EntregaNombre) ? "Ing. Baldemar López" : dto.EntregaNombre.Trim(),
            EntregaEmpresa = string.IsNullOrWhiteSpace(dto.EntregaEmpresa) ? "Servicios y Proyectos Industriales BLL" : dto.EntregaEmpresa.Trim(),
            RecibeNombre = dto.RecibeNombre,
            RecibeEmpresa = dto.RecibeEmpresa,
            CreadoPorId = uid,
            FechaCreacion = DateTime.UtcNow
        };

        if (dto.ProyectoId.HasValue)
            entity.Proyecto = await _context.Proyectos.FindAsync(dto.ProyectoId.Value);
        if (dto.ClienteId.HasValue)
            entity.Cliente = await _context.Clientes.FindAsync(dto.ClienteId.Value);

        _context.ReportesEntregaConformidad.Add(entity);
        await _context.SaveChangesAsync();

        await _bitacora.RegistrarAsync(uid, uname, "Creó reporte de entrega y conformidad", "ReporteEntregaConformidad",
            $"Reporte #{entity.Folio} ({entity.Concepto}) creado.", ip);

        return (true, "Reporte creado.", MapToDto(entity));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var entity = await _context.ReportesEntregaConformidad.FirstOrDefaultAsync(r => r.Id == id);
        if (entity is null) return (false, "Reporte no encontrado.");

        _context.ReportesEntregaConformidad.Remove(entity);
        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó reporte de entrega y conformidad", "ReporteEntregaConformidad",
            $"Reporte #{entity.Folio} (ID {id}) eliminado.", ip);

        return (true, "Reporte eliminado.");
    }

    public async Task<(bool Success, string Message)> AgregarFotosAsync(int id, List<IFormFile> fotos)
    {
        var existe = await _context.ReportesEntregaConformidad.AnyAsync(r => r.Id == id);
        if (!existe) return (false, "Reporte no encontrado.");

        if (fotos.Count == 0) return (false, "Selecciona al menos una foto.");

        foreach (var foto in fotos)
        {
            if (foto.Length == 0) continue;
            if (foto.Length > MaxTamanioBytes) return (false, $"'{foto.FileName}' supera el límite de 20 MB.");

            using var ms = new MemoryStream();
            await foto.CopyToAsync(ms);

            _context.ReportesEntregaConformidadFotos.Add(new Models.Reportes.ReporteEntregaConformidadFoto
            {
                ReporteEntregaConformidadId = id,
                NombreOriginal = foto.FileName,
                ContentType = foto.ContentType,
                TamanioBytes = foto.Length,
                Contenido = ms.ToArray(),
                FechaCaptura = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return (true, "Fotos agregadas.");
    }

    public async Task<(bool Found, string ContentType, byte[]? Contenido)> DescargarFotoAsync(int fotoId)
    {
        var foto = await _context.ReportesEntregaConformidadFotos.AsNoTracking().FirstOrDefaultAsync(f => f.Id == fotoId);
        return foto is null ? (false, string.Empty, null) : (true, foto.ContentType, foto.Contenido);
    }

    public async Task<(bool Success, string Message)> EliminarFotoAsync(int fotoId)
    {
        var foto = await _context.ReportesEntregaConformidadFotos.FirstOrDefaultAsync(f => f.Id == fotoId);
        if (foto is null) return (false, "Foto no encontrada.");

        _context.ReportesEntregaConformidadFotos.Remove(foto);
        await _context.SaveChangesAsync();
        return (true, "Foto eliminada.");
    }

    public async Task<(bool Success, string Message)> SubirDocumentoFirmadoAsync(int id, IFormFile archivo)
    {
        var entity = await _context.ReportesEntregaConformidad.FirstOrDefaultAsync(r => r.Id == id);
        if (entity is null) return (false, "Reporte no encontrado.");

        if (archivo.Length == 0) return (false, "El archivo está vacío.");
        if (archivo.Length > MaxTamanioBytes) return (false, "El archivo supera el límite de 20 MB.");

        using var ms = new MemoryStream();
        await archivo.CopyToAsync(ms);

        entity.DocumentoFirmadoNombre = archivo.FileName;
        entity.DocumentoFirmadoContentType = archivo.ContentType;
        entity.DocumentoFirmadoContenido = ms.ToArray();
        entity.DocumentoFirmadoFecha = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Subió documento firmado", "ReporteEntregaConformidad",
            $"Documento firmado y escaneado subido para el reporte #{entity.Folio} (ID {id}).", ip);

        return (true, "Documento firmado guardado.");
    }

    public async Task<(bool Found, string? NombreOriginal, string? ContentType, byte[]? Contenido)> DescargarDocumentoFirmadoAsync(int id)
    {
        var entity = await _context.ReportesEntregaConformidad.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        if (entity?.DocumentoFirmadoContenido is null) return (false, null, null, null);
        return (true, entity.DocumentoFirmadoNombre, entity.DocumentoFirmadoContentType, entity.DocumentoFirmadoContenido);
    }

    public async Task<(bool Success, string Message, byte[]? Pdf)> GenerarPdfAsync(int id)
    {
        var entity = await _context.ReportesEntregaConformidad
            .AsNoTracking()
            .Include(r => r.Proyecto)
            .Include(r => r.Cliente)
            .Include(r => r.Fotos)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (entity is null) return (false, "Reporte no encontrado.", null);

        var documento = new ReporteEntregaConformidadDocument(entity);
        var pdf = documento.GeneratePdf();
        return (true, "OK", pdf);
    }
}
