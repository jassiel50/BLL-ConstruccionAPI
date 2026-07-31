using System.Security.Claims;
using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Personal;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Personal;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class EmpleadosService : IEmpleadosService
{
    private static readonly long MaxTamanioBytes = 20 * 1024 * 1024; // 20 MB

    private readonly AppDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EmpleadosService(AppDbContext context, IBitacoraService bitacora, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
    }

    private (int Id, string Nombre) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        return (id, nombre);
    }

    // ─── EMPLEADOS ──────────────────────────────────────────────────────────

    public async Task<List<EmpleadoResponseDto>> GetAllAsync()
    {
        var empleados = await _context.Empleados
            .AsNoTracking()
            .OrderBy(e => e.NombreCompleto)
            .ToListAsync();

        var proyectosActivos = await _context.AsignacionesEmpleadoProyecto
            .AsNoTracking()
            .Include(a => a.Proyecto)
            .Where(a => a.Estado == EstadoAsignacionEmpleado.Activa)
            .ToDictionaryAsync(a => a.EmpleadoId, a => a);

        return empleados.Select(e =>
        {
            proyectosActivos.TryGetValue(e.Id, out var asignacion);
            return EmpleadoResponseDto.FromEntity(e, asignacion?.Proyecto?.Nombre, asignacion?.ProyectoId);
        }).ToList();
    }

    public async Task<EmpleadoResponseDto?> GetByIdAsync(int id)
    {
        var empleado = await _context.Empleados.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (empleado is null) return null;

        var asignacionActiva = await _context.AsignacionesEmpleadoProyecto
            .AsNoTracking()
            .Include(a => a.Proyecto)
            .FirstOrDefaultAsync(a => a.EmpleadoId == id && a.Estado == EstadoAsignacionEmpleado.Activa);

        return EmpleadoResponseDto.FromEntity(empleado, asignacionActiva?.Proyecto?.Nombre, asignacionActiva?.ProyectoId);
    }

    public async Task<(bool Success, string Message, EmpleadoResponseDto? Data)> CreateAsync(EmpleadoRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NumeroEmpleado))
            return (false, "El número de empleado es requerido.", null);
        if (string.IsNullOrWhiteSpace(dto.NombreCompleto))
            return (false, "El nombre completo es requerido.", null);

        var yaExiste = await _context.Empleados.AnyAsync(e => e.NumeroEmpleado == dto.NumeroEmpleado);
        if (yaExiste)
            return (false, $"Ya existe un empleado con el número '{dto.NumeroEmpleado}'.", null);

        var empleado = new Empleado
        {
            NumeroEmpleado         = dto.NumeroEmpleado.Trim(),
            NombreCompleto         = dto.NombreCompleto.Trim(),
            Puesto                 = dto.Puesto?.Trim() ?? string.Empty,
            CURP                   = dto.CURP,
            RFC                    = dto.RFC,
            NSS                    = dto.NSS,
            Telefono               = dto.Telefono,
            ContactoEmergencia     = dto.ContactoEmergencia,
            FechaIngreso           = dto.FechaIngreso,
            Estatus                = EstatusEmpleado.Activo,
            SueldoNetoSemanal      = dto.SueldoNetoSemanal,
            CreditoInfonavit       = dto.CreditoInfonavit,
            TipoDescuentoInfonavit = dto.TipoDescuentoInfonavit,
            CuotaInfonavit         = dto.CuotaInfonavit,
            Observaciones          = dto.Observaciones,
            FechaRegistro          = DateTime.UtcNow
        };

        _context.Empleados.Add(empleado);
        await _context.SaveChangesAsync();

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Registró", "Empleado", $"Nuevo empleado '{empleado.NombreCompleto}' ({empleado.NumeroEmpleado})");

        return (true, "Empleado registrado correctamente.", EmpleadoResponseDto.FromEntity(empleado));
    }

    public async Task<(bool Success, string Message, EmpleadoResponseDto? Data)> UpdateAsync(int id, EmpleadoRequestDto dto)
    {
        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.Id == id);
        if (empleado is null) return (false, "Empleado no encontrado.", null);

        if (string.IsNullOrWhiteSpace(dto.NumeroEmpleado))
            return (false, "El número de empleado es requerido.", null);
        if (string.IsNullOrWhiteSpace(dto.NombreCompleto))
            return (false, "El nombre completo es requerido.", null);

        var yaExiste = await _context.Empleados.AnyAsync(e => e.NumeroEmpleado == dto.NumeroEmpleado && e.Id != id);
        if (yaExiste)
            return (false, $"Ya existe otro empleado con el número '{dto.NumeroEmpleado}'.", null);

        empleado.NumeroEmpleado         = dto.NumeroEmpleado.Trim();
        empleado.NombreCompleto         = dto.NombreCompleto.Trim();
        empleado.Puesto                 = dto.Puesto?.Trim() ?? string.Empty;
        empleado.CURP                   = dto.CURP;
        empleado.RFC                    = dto.RFC;
        empleado.NSS                    = dto.NSS;
        empleado.Telefono               = dto.Telefono;
        empleado.ContactoEmergencia     = dto.ContactoEmergencia;
        empleado.FechaIngreso           = dto.FechaIngreso;
        empleado.SueldoNetoSemanal      = dto.SueldoNetoSemanal;
        empleado.CreditoInfonavit       = dto.CreditoInfonavit;
        empleado.TipoDescuentoInfonavit = dto.TipoDescuentoInfonavit;
        empleado.CuotaInfonavit         = dto.CuotaInfonavit;
        empleado.Observaciones          = dto.Observaciones;

        await _context.SaveChangesAsync();

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Actualizó", "Empleado", $"Empleado '{empleado.NombreCompleto}' ({empleado.NumeroEmpleado}) actualizado");

        return (true, "Empleado actualizado correctamente.", EmpleadoResponseDto.FromEntity(empleado));
    }

    public async Task<(bool Success, string Message)> ToggleEstatusAsync(int id)
    {
        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.Id == id);
        if (empleado is null) return (false, "Empleado no encontrado.");

        empleado.Estatus = empleado.Estatus == EstatusEmpleado.Activo ? EstatusEmpleado.Baja : EstatusEmpleado.Activo;
        await _context.SaveChangesAsync();

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, empleado.Estatus == EstatusEmpleado.Baja ? "Dio de baja" : "Reactivó", "Empleado",
            $"Empleado '{empleado.NombreCompleto}' ({empleado.NumeroEmpleado}) ahora está {empleado.Estatus}");

        return (true, $"Empleado marcado como {empleado.Estatus}.");
    }

    // ─── ARCHIVOS ───────────────────────────────────────────────────────────

    public async Task<List<ArchivoEmpleadoDto>> GetArchivosAsync(int empleadoId) =>
        await _context.ArchivosEmpleado
            .AsNoTracking()
            .Where(a => a.EmpleadoId == empleadoId)
            .OrderByDescending(a => a.FechaSubida)
            .Select(a => ArchivoEmpleadoDto.FromEntity(a))
            .ToListAsync();

    public async Task<(bool Success, string Message, ArchivoEmpleadoDto? Data)> SubirArchivoAsync(int empleadoId, IFormFile archivo, string tipoDocumento)
    {
        var empleadoExiste = await _context.Empleados.AnyAsync(e => e.Id == empleadoId);
        if (!empleadoExiste) return (false, "Empleado no encontrado.", null);

        if (archivo.Length == 0) return (false, "El archivo está vacío.", null);
        if (archivo.Length > MaxTamanioBytes) return (false, "El archivo supera el límite de 20 MB.", null);

        if (!Enum.TryParse<TipoDocumentoEmpleado>(tipoDocumento, out var tipo))
            return (false, $"TipoDocumento inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<TipoDocumentoEmpleado>())}.", null);

        using var ms = new MemoryStream();
        await archivo.CopyToAsync(ms);

        var (uid, uname) = GetUsuarioInfo();
        var entity = new ArchivoEmpleado
        {
            EmpleadoId     = empleadoId,
            NombreOriginal = archivo.FileName,
            TipoDocumento  = tipo,
            ContentType    = archivo.ContentType,
            TamanioBytes   = archivo.Length,
            Contenido      = ms.ToArray(),
            SubidoPorId    = uid,
            FechaSubida    = DateTime.UtcNow
        };

        _context.ArchivosEmpleado.Add(entity);
        await _context.SaveChangesAsync();

        await _bitacora.RegistrarAsync(uid, uname, "Subió archivo", "Empleado", $"Archivo '{entity.NombreOriginal}' ({tipo}) subido al empleado ID {empleadoId}");

        return (true, "Archivo subido correctamente.", ArchivoEmpleadoDto.FromEntity(entity));
    }

    public async Task<(bool Found, string NombreOriginal, string ContentType, byte[]? Contenido)> DescargarArchivoAsync(int id)
    {
        var archivo = await _context.ArchivosEmpleado.FindAsync(id);
        if (archivo is null) return (false, "", "", null);
        return (true, archivo.NombreOriginal, archivo.ContentType, archivo.Contenido);
    }

    public async Task<(bool Success, string Message)> EliminarArchivoAsync(int id)
    {
        var archivo = await _context.ArchivosEmpleado.FindAsync(id);
        if (archivo is null) return (false, "Archivo no encontrado.");

        _context.ArchivosEmpleado.Remove(archivo);
        await _context.SaveChangesAsync();

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Eliminó archivo", "Empleado", $"Archivo '{archivo.NombreOriginal}' (ID {archivo.Id}) eliminado");

        return (true, "Archivo eliminado.");
    }

    // ─── ASIGNACIONES A PROYECTO ────────────────────────────────────────────

    public async Task<List<AsignacionEmpleadoResponseDto>> GetAsignacionesAsync(int empleadoId) =>
        await _context.AsignacionesEmpleadoProyecto
            .AsNoTracking()
            .Include(a => a.Proyecto)
            .Include(a => a.Empleado)
            .Where(a => a.EmpleadoId == empleadoId)
            .OrderByDescending(a => a.FechaAsignacion)
            .Select(a => AsignacionEmpleadoResponseDto.FromEntity(a))
            .ToListAsync();

    public async Task<(bool Success, string Message, AsignacionEmpleadoResponseDto? Data)> AsignarAsync(int empleadoId, AsignacionEmpleadoRequestDto dto, int usuarioId)
    {
        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.Id == empleadoId);
        if (empleado is null) return (false, "Empleado no encontrado.", null);

        var proyecto = await _context.Proyectos.FirstOrDefaultAsync(p => p.Id == dto.ProyectoId);
        if (proyecto is null) return (false, "El proyecto especificado no existe.", null);

        var yaAsignado = await _context.AsignacionesEmpleadoProyecto
            .AnyAsync(a => a.EmpleadoId == empleadoId && a.Estado == EstadoAsignacionEmpleado.Activa);
        if (yaAsignado)
            return (false, "Este empleado ya está asignado activamente a un proyecto. Finaliza esa asignación antes de crear una nueva.", null);

        var asignacion = new AsignacionEmpleadoProyecto
        {
            EmpleadoId      = empleadoId,
            ProyectoId      = dto.ProyectoId,
            UsuarioAsignoId = usuarioId,
            FechaAsignacion = DateTime.UtcNow,
            Estado          = EstadoAsignacionEmpleado.Activa,
            Observaciones   = dto.Observaciones
        };

        _context.AsignacionesEmpleadoProyecto.Add(asignacion);
        await _context.SaveChangesAsync();

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Asignó", "Empleado", $"'{empleado.NombreCompleto}' asignado al proyecto '{proyecto.Nombre}'");

        var result = await _context.AsignacionesEmpleadoProyecto
            .AsNoTracking()
            .Include(a => a.Proyecto)
            .Include(a => a.Empleado)
            .FirstAsync(a => a.Id == asignacion.Id);

        return (true, "Empleado asignado correctamente.", AsignacionEmpleadoResponseDto.FromEntity(result));
    }

    public async Task<(bool Success, string Message)> FinalizarAsignacionAsync(int asignacionId, AsignacionEmpleadoFinalizarDto dto)
    {
        var asignacion = await _context.AsignacionesEmpleadoProyecto
            .Include(a => a.Empleado)
            .Include(a => a.Proyecto)
            .FirstOrDefaultAsync(a => a.Id == asignacionId);
        if (asignacion is null) return (false, "Asignación no encontrada.");

        if (asignacion.Estado != EstadoAsignacionEmpleado.Activa)
            return (false, "Esta asignación ya fue finalizada.");

        asignacion.Estado           = EstadoAsignacionEmpleado.Finalizada;
        asignacion.FechaFin         = DateTime.UtcNow;
        asignacion.ObservacionesFin = dto.ObservacionesFin;

        await _context.SaveChangesAsync();

        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Finalizó asignación", "Empleado",
            $"'{asignacion.Empleado?.NombreCompleto}' finalizó su asignación al proyecto '{asignacion.Proyecto?.Nombre}'");

        return (true, "Asignación finalizada correctamente.");
    }
}
