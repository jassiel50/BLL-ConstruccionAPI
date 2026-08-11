using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Servicios;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Servicios;
using BLL_ConstruccionAPI.Reports;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace BLL_ConstruccionAPI.Services;

public class ServiciosService : IServiciosService
{
    private const int RolOperadorServicio = 4;
    private const int LigaDuracionHoras = 24;

    private readonly AppDbContext _context;
    private readonly IBitacoraService _bitacora;

    public ServiciosService(AppDbContext context, IBitacoraService bitacora)
    {
        _context = context;
        _bitacora = bitacora;
    }

    public async Task<IEnumerable<ServicioResponseDto>> GetAllAsync(int solicitanteRolId, int solicitanteUsuarioId)
    {
        var query = _context.Servicios
            .AsNoTracking()
            .Include(s => s.Cliente)
            .AsQueryable();

        if (solicitanteRolId == RolOperadorServicio)
            query = query.Where(s => s.OperadorId == solicitanteUsuarioId);

        var servicios = await query
            .OrderByDescending(s => s.FechaCreacion)
            .ToListAsync();

        return servicios.Select(ServicioResponseDto.FromEntity);
    }

    public async Task<ServicioResponseDto?> GetByIdAsync(int id, int solicitanteRolId, int solicitanteUsuarioId)
    {
        var servicio = await _context.Servicios
            .AsNoTracking()
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (servicio is null) return null;
        if (solicitanteRolId == RolOperadorServicio && servicio.OperadorId != solicitanteUsuarioId) return null;

        return ServicioResponseDto.FromEntity(servicio);
    }

    public async Task<(bool Success, string Message, ServicioResponseDto? Data)> CreateAsync(int usuarioId, string nombreUsuario, ServicioRequestDto dto)
    {
        // Tipo ya no es obligatorio al crear — lo define el técnico desde la liga pública.
        TipoServicio? tipo = null;
        if (!string.IsNullOrWhiteSpace(dto.Tipo))
        {
            if (!Enum.TryParse<TipoServicio>(dto.Tipo, ignoreCase: true, out var tipoParseado))
                return (false, $"Tipo de servicio inválido: '{dto.Tipo}'.", null);
            tipo = tipoParseado;
        }

        if (dto.ClienteId is null && string.IsNullOrWhiteSpace(dto.ClienteNombre))
            return (false, "Debes seleccionar un cliente del catálogo o capturar los datos de un cliente nuevo.", null);

        if (dto.ClienteId is not null)
        {
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == dto.ClienteId);
            if (!clienteExiste)
                return (false, "El cliente seleccionado no existe.", null);
        }

        var servicio = new Servicio
        {
            ClienteId            = dto.ClienteId,
            ClienteNombre        = dto.ClienteNombre,
            ClienteDireccion     = dto.ClienteDireccion,
            ClienteTelefono      = dto.ClienteTelefono,
            Tipo                 = tipo,
            Equipo               = dto.Equipo,
            DireccionServicio    = dto.DireccionServicio,
            DescripcionTrabajo   = dto.DescripcionTrabajo,
            MaterialesUtilizados = dto.MaterialesUtilizados,
            Observaciones        = dto.Observaciones,
            Estado               = EstadoServicio.Activo,
            OperadorId           = usuarioId,
            OperadorNombre       = nombreUsuario,
            FechaInicio          = DateTime.UtcNow,
            FechaCreacion        = DateTime.UtcNow,
            HorarioTrabajo         = dto.HorarioTrabajo,
            NumeroTrabajadores     = dto.NumeroTrabajadores,
            TotalHorasTrabajadas   = dto.TotalHorasTrabajadas,
            RecursoManoDeObra      = dto.RecursoManoDeObra,
            RecursoHerramienta     = dto.RecursoHerramienta,
            RecursoRefacciones     = dto.RecursoRefacciones,
            RecursoConsumibles     = dto.RecursoConsumibles,
            TiposTrabajo           = dto.TiposTrabajo is { Count: > 0 } ? string.Join(',', dto.TiposTrabajo) : null
        };

        _context.Servicios.Add(servicio);
        await _context.SaveChangesAsync();

        var result = await _context.Servicios
            .AsNoTracking()
            .Include(s => s.Cliente)
            .FirstAsync(s => s.Id == servicio.Id);

        var nombreCliente = result.ClienteId.HasValue && result.Cliente is not null ? result.Cliente.Nombre : result.ClienteNombre;
        await _bitacora.RegistrarAsync(usuarioId, nombreUsuario, "Registró", "Servicio", $"Nuevo servicio para '{nombreCliente}'");

        return (true, "Servicio registrado correctamente.", ServicioResponseDto.FromEntity(result));
    }

    public async Task<(bool Success, string Message, ServicioResponseDto? Data)> UpdateAsync(int id, int usuarioId, ServicioRequestDto dto)
    {
        var servicio = await _context.Servicios.FirstOrDefaultAsync(s => s.Id == id);
        if (servicio is null)
            return (false, "Servicio no encontrado.", null);

        if (servicio.OperadorId != usuarioId)
            return (false, "No tienes permiso para modificar este servicio.", null);

        if (servicio.Estado != EstadoServicio.Activo)
            return (false, "El servicio ya fue firmado/finalizado y no se puede modificar.", null);

        TipoServicio? tipo = null;
        if (!string.IsNullOrWhiteSpace(dto.Tipo))
        {
            if (!Enum.TryParse<TipoServicio>(dto.Tipo, ignoreCase: true, out var tipoParseado))
                return (false, $"Tipo de servicio inválido: '{dto.Tipo}'.", null);
            tipo = tipoParseado;
        }

        if (dto.ClienteId is null && string.IsNullOrWhiteSpace(dto.ClienteNombre))
            return (false, "Debes seleccionar un cliente del catálogo o capturar los datos de un cliente nuevo.", null);

        servicio.ClienteId            = dto.ClienteId;
        servicio.ClienteNombre        = dto.ClienteNombre;
        servicio.ClienteDireccion     = dto.ClienteDireccion;
        servicio.ClienteTelefono      = dto.ClienteTelefono;
        servicio.Tipo                 = tipo;
        servicio.Equipo               = dto.Equipo;
        servicio.DireccionServicio    = dto.DireccionServicio;
        servicio.DescripcionTrabajo   = dto.DescripcionTrabajo;
        servicio.MaterialesUtilizados = dto.MaterialesUtilizados;
        servicio.Observaciones        = dto.Observaciones;

        await _context.SaveChangesAsync();

        var result = await _context.Servicios
            .AsNoTracking()
            .Include(s => s.Cliente)
            .FirstAsync(s => s.Id == id);

        return (true, "Servicio actualizado correctamente.", ServicioResponseDto.FromEntity(result));
    }

    public async Task<(bool Success, string Message, ServicioResponseDto? Data)> FirmarAsync(int id, int usuarioId, ServicioFirmarDto dto)
    {
        var servicio = await _context.Servicios.FirstOrDefaultAsync(s => s.Id == id);
        if (servicio is null)
            return (false, "Servicio no encontrado.", null);

        if (servicio.OperadorId != usuarioId)
            return (false, "No tienes permiso para firmar este servicio.", null);

        if (servicio.Estado != EstadoServicio.Activo)
            return (false, "Este servicio ya fue finalizado.", null);

        if (string.IsNullOrWhiteSpace(dto.FirmaBase64))
            return (false, "La firma es requerida para finalizar el servicio.", null);

        if (string.IsNullOrWhiteSpace(dto.NombreQuienFirma))
            return (false, "El nombre de quien firma es requerido.", null);

        var totalFotos = await _context.ServiciosFotos.CountAsync(f => f.ServicioId == id);
        if (totalFotos < 3)
            return (false, "Se requieren al menos 3 fotos de evidencia antes de finalizar el servicio.", null);

        servicio.FirmaBase64      = dto.FirmaBase64;
        servicio.NombreQuienFirma = dto.NombreQuienFirma;
        servicio.FechaFirma       = DateTime.UtcNow;
        servicio.FechaFin         = DateTime.UtcNow;
        servicio.Estado           = EstadoServicio.Terminado;

        await _context.SaveChangesAsync();

        var result = await _context.Servicios
            .AsNoTracking()
            .Include(s => s.Cliente)
            .FirstAsync(s => s.Id == id);

        await _bitacora.RegistrarAsync(usuarioId, servicio.OperadorNombre, "Firmó", "Servicio", $"Servicio #{id} finalizado y firmado por {dto.NombreQuienFirma}");

        return (true, "Servicio finalizado y firmado correctamente.", ServicioResponseDto.FromEntity(result));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id, int usuarioId)
    {
        var servicio = await _context.Servicios.FirstOrDefaultAsync(s => s.Id == id);
        if (servicio is null)
            return (false, "Servicio no encontrado.");

        if (servicio.OperadorId != usuarioId)
            return (false, "No tienes permiso para eliminar este servicio.");

        if (servicio.Estado != EstadoServicio.Activo)
            return (false, "No se puede eliminar un servicio ya finalizado/firmado.");

        _context.Servicios.Remove(servicio);
        await _context.SaveChangesAsync();

        await _bitacora.RegistrarAsync(usuarioId, servicio.OperadorNombre, "Eliminó", "Servicio", $"Servicio #{id} eliminado");
        return (true, "Servicio eliminado correctamente.");
    }

    // ─── EVIDENCIAS FOTOGRÁFICAS ────────────────────────────────────────────

    private static readonly long MaxTamanioFotoBytes = 20 * 1024 * 1024; // 20 MB

    public async Task<(bool Success, string Message, List<ServicioFotoDto> Data)> GetFotosAsync(int servicioId, int solicitanteRolId, int solicitanteUsuarioId)
    {
        var servicio = await _context.Servicios.AsNoTracking().FirstOrDefaultAsync(s => s.Id == servicioId);
        if (servicio is null) return (false, "Servicio no encontrado.", []);

        if (solicitanteRolId == RolOperadorServicio && servicio.OperadorId != solicitanteUsuarioId)
            return (false, "No tienes permiso para ver las evidencias de este servicio.", []);

        var fotos = await _context.ServiciosFotos
            .AsNoTracking()
            .Where(f => f.ServicioId == servicioId)
            .OrderBy(f => f.FechaCaptura)
            .Select(f => ServicioFotoDto.FromEntity(f))
            .ToListAsync();

        return (true, "OK", fotos);
    }

    public async Task<(bool Success, string Message, ServicioFotoDto? Data)> SubirFotoAsync(int servicioId, int usuarioId, IFormFile foto)
    {
        var servicio = await _context.Servicios.AsNoTracking().FirstOrDefaultAsync(s => s.Id == servicioId);
        if (servicio is null) return (false, "Servicio no encontrado.", null);

        if (servicio.OperadorId != usuarioId)
            return (false, "No tienes permiso para agregar evidencias a este servicio.", null);

        if (servicio.Estado != EstadoServicio.Activo)
            return (false, "El servicio ya fue firmado/finalizado y no admite más evidencias.", null);

        if (foto.Length == 0) return (false, "El archivo está vacío.", null);
        if (foto.Length > MaxTamanioFotoBytes) return (false, "La foto supera el límite de 20 MB.", null);

        using var ms = new MemoryStream();
        await foto.CopyToAsync(ms);

        var entity = new ServicioFoto
        {
            ServicioId     = servicioId,
            NombreOriginal = foto.FileName,
            ContentType    = foto.ContentType,
            TamanioBytes   = foto.Length,
            Contenido      = ms.ToArray(),
            FechaCaptura   = DateTime.UtcNow
        };

        _context.ServiciosFotos.Add(entity);
        await _context.SaveChangesAsync();

        return (true, "Foto subida correctamente.", ServicioFotoDto.FromEntity(entity));
    }

    public async Task<(bool Found, string NombreOriginal, string ContentType, byte[]? Contenido)> DescargarFotoAsync(int fotoId, int solicitanteRolId, int solicitanteUsuarioId)
    {
        var foto = await _context.ServiciosFotos
            .Include(f => f.Servicio)
            .FirstOrDefaultAsync(f => f.Id == fotoId);
        if (foto is null) return (false, "", "", null);

        if (solicitanteRolId == RolOperadorServicio && foto.Servicio?.OperadorId != solicitanteUsuarioId)
            return (false, "", "", null);

        return (true, foto.NombreOriginal, foto.ContentType, foto.Contenido);
    }

    public async Task<(bool Success, string Message)> EliminarFotoAsync(int fotoId, int usuarioId)
    {
        var foto = await _context.ServiciosFotos
            .Include(f => f.Servicio)
            .FirstOrDefaultAsync(f => f.Id == fotoId);
        if (foto is null) return (false, "Foto no encontrada.");

        if (foto.Servicio is not null && foto.Servicio.OperadorId != usuarioId)
            return (false, "No tienes permiso para eliminar esta evidencia.");

        if (foto.Servicio is not null && foto.Servicio.Estado != EstadoServicio.Activo)
            return (false, "El servicio ya fue firmado/finalizado y no se pueden eliminar evidencias.");

        _context.ServiciosFotos.Remove(foto);
        await _context.SaveChangesAsync();

        return (true, "Foto eliminada.");
    }

    // ─── REPORTE PDF ────────────────────────────────────────────────────────

    public async Task<byte[]> GenerarReporteAsync(int servicioId, int solicitanteRolId, int solicitanteUsuarioId)
    {
        var servicio = await _context.Servicios
            .AsNoTracking()
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == servicioId);

        if (servicio is null || servicio.Estado != EstadoServicio.Terminado)
            return [];

        if (solicitanteRolId == RolOperadorServicio && servicio.OperadorId != solicitanteUsuarioId)
            return [];

        var fotos = await _context.ServiciosFotos
            .AsNoTracking()
            .Where(f => f.ServicioId == servicioId)
            .OrderBy(f => f.FechaCaptura)
            .ToListAsync();

        return Document.Create(container =>
                new ReporteServicioDocument(servicio, fotos).Compose(container))
            .GeneratePdf();
    }

    // ─── LIGA PÚBLICA (operador interno) ───────────────────────────────────

    public async Task<(bool Success, string Message, ServicioLigaDto? Data)> GenerarLigaAsync(int servicioId, int usuarioId)
    {
        var servicio = await _context.Servicios.FirstOrDefaultAsync(s => s.Id == servicioId);
        if (servicio is null) return (false, "Servicio no encontrado.", null);

        if (servicio.OperadorId != usuarioId)
            return (false, "No tienes permiso para generar una liga para este servicio.", null);

        if (servicio.Estado != EstadoServicio.Activo)
            return (false, "El servicio ya fue finalizado; no se puede generar una liga.", null);

        servicio.TokenPublico = Guid.NewGuid().ToString("N");
        servicio.TokenExpira = DateTime.UtcNow.AddHours(LigaDuracionHoras);
        await _context.SaveChangesAsync();

        await _bitacora.RegistrarAsync(usuarioId, servicio.OperadorNombre, "Generó", "Servicio", $"Liga pública generada para el servicio #{servicioId}");

        return (true, "Liga generada correctamente.", new ServicioLigaDto { Token = servicio.TokenPublico, Expira = servicio.TokenExpira.Value });
    }

    public async Task<ServicioLigaDto?> GetLigaActualAsync(int servicioId, int usuarioId)
    {
        var servicio = await _context.Servicios.AsNoTracking().FirstOrDefaultAsync(s => s.Id == servicioId);
        if (servicio is null || servicio.OperadorId != usuarioId) return null;

        if (string.IsNullOrWhiteSpace(servicio.TokenPublico) || servicio.TokenExpira is null || servicio.TokenExpira < DateTime.UtcNow)
            return null;

        return new ServicioLigaDto { Token = servicio.TokenPublico, Expira = servicio.TokenExpira.Value };
    }

    // ─── ACCESO POR TOKEN (sin sesión) ─────────────────────────────────────

    private async Task<(Servicio? Servicio, string Motivo)> ValidarTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return (null, "Liga inválida.");

        var servicio = await _context.Servicios
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.TokenPublico == token);

        if (servicio is null) return (null, "Esta liga no es válida o ya fue utilizada.");
        if (servicio.Estado != EstadoServicio.Activo) return (null, "Este servicio ya fue finalizado; la liga ya no está disponible.");
        if (servicio.TokenExpira is null || servicio.TokenExpira < DateTime.UtcNow) return (null, "Esta liga expiró. Pide que te generen una nueva.");

        return (servicio, "");
    }

    public async Task<(bool Valido, string Motivo, ServicioPublicoDto? Data)> GetPorTokenAsync(string token)
    {
        var (servicio, motivo) = await ValidarTokenAsync(token);
        if (servicio is null) return (false, motivo, null);

        return (true, "", ServicioPublicoDto.FromEntity(servicio));
    }

    public async Task<(bool Success, string Message, ServicioPublicoDto? Data)> ActualizarPorTokenAsync(string token, ServicioPublicoUpdateDto dto)
    {
        var (servicio, motivo) = await ValidarTokenAsync(token);
        if (servicio is null) return (false, motivo, null);

        if (!string.IsNullOrWhiteSpace(dto.Tipo))
        {
            if (!Enum.TryParse<TipoServicio>(dto.Tipo, ignoreCase: true, out var tipoParseado))
                return (false, $"Tipo de servicio inválido: '{dto.Tipo}'.", null);
            servicio.Tipo = tipoParseado;
        }
        if (dto.DireccionServicio is not null)
            servicio.DireccionServicio = dto.DireccionServicio;
        if (dto.NombreSolicitante is not null)
            servicio.NombreSolicitante = dto.NombreSolicitante;
        if (!string.IsNullOrWhiteSpace(dto.OperadorNombre))
            servicio.OperadorNombre = dto.OperadorNombre;

        servicio.Equipo               = dto.Equipo;
        servicio.DescripcionTrabajo   = dto.DescripcionTrabajo;
        servicio.MaterialesUtilizados = dto.MaterialesUtilizados;
        servicio.Observaciones        = dto.Observaciones;
        servicio.HorarioTrabajo       = dto.HorarioTrabajo;
        servicio.NumeroTrabajadores   = dto.NumeroTrabajadores;
        servicio.TotalHorasTrabajadas = dto.TotalHorasTrabajadas;
        servicio.RecursoManoDeObra    = dto.RecursoManoDeObra;
        servicio.RecursoHerramienta   = dto.RecursoHerramienta;
        servicio.RecursoRefacciones   = dto.RecursoRefacciones;
        servicio.RecursoConsumibles   = dto.RecursoConsumibles;
        servicio.TiposTrabajo         = dto.TiposTrabajo is { Count: > 0 } ? string.Join(',', dto.TiposTrabajo) : null;

        await _context.SaveChangesAsync();

        var result = await _context.Servicios.AsNoTracking().Include(s => s.Cliente).FirstAsync(s => s.Id == servicio.Id);
        return (true, "Datos guardados correctamente.", ServicioPublicoDto.FromEntity(result));
    }

    public async Task<(bool Success, string Message, List<ServicioFotoDto> Data)> GetFotosPorTokenAsync(string token)
    {
        var (servicio, motivo) = await ValidarTokenAsync(token);
        if (servicio is null) return (false, motivo, []);

        var fotos = await _context.ServiciosFotos
            .AsNoTracking()
            .Where(f => f.ServicioId == servicio.Id)
            .OrderBy(f => f.FechaCaptura)
            .Select(f => ServicioFotoDto.FromEntity(f))
            .ToListAsync();

        return (true, "OK", fotos);
    }

    public async Task<(bool Success, string Message, ServicioFotoDto? Data)> SubirFotoPorTokenAsync(string token, IFormFile foto)
    {
        var (servicio, motivo) = await ValidarTokenAsync(token);
        if (servicio is null) return (false, motivo, null);

        if (foto.Length == 0) return (false, "El archivo está vacío.", null);
        if (foto.Length > MaxTamanioFotoBytes) return (false, "La foto supera el límite de 20 MB.", null);

        using var ms = new MemoryStream();
        await foto.CopyToAsync(ms);

        var entity = new ServicioFoto
        {
            ServicioId     = servicio.Id,
            NombreOriginal = foto.FileName,
            ContentType    = foto.ContentType,
            TamanioBytes   = foto.Length,
            Contenido      = ms.ToArray(),
            FechaCaptura   = DateTime.UtcNow
        };

        _context.ServiciosFotos.Add(entity);
        await _context.SaveChangesAsync();

        return (true, "Foto subida correctamente.", ServicioFotoDto.FromEntity(entity));
    }

    public async Task<(bool Found, string NombreOriginal, string ContentType, byte[]? Contenido)> DescargarFotoPorTokenAsync(string token, int fotoId)
    {
        var (servicio, _) = await ValidarTokenAsync(token);
        if (servicio is null) return (false, "", "", null);

        var foto = await _context.ServiciosFotos.FirstOrDefaultAsync(f => f.Id == fotoId && f.ServicioId == servicio.Id);
        if (foto is null) return (false, "", "", null);

        return (true, foto.NombreOriginal, foto.ContentType, foto.Contenido);
    }

    public async Task<(bool Success, string Message)> EliminarFotoPorTokenAsync(string token, int fotoId)
    {
        var (servicio, motivo) = await ValidarTokenAsync(token);
        if (servicio is null) return (false, motivo);

        var foto = await _context.ServiciosFotos.FirstOrDefaultAsync(f => f.Id == fotoId && f.ServicioId == servicio.Id);
        if (foto is null) return (false, "Foto no encontrada.");

        _context.ServiciosFotos.Remove(foto);
        await _context.SaveChangesAsync();

        return (true, "Foto eliminada.");
    }

    public async Task<(bool Success, string Message, ServicioPublicoDto? Data)> FirmarPorTokenAsync(string token, ServicioFirmarDto dto)
    {
        var (servicio, motivo) = await ValidarTokenAsync(token);
        if (servicio is null) return (false, motivo, null);

        if (string.IsNullOrWhiteSpace(dto.FirmaBase64))
            return (false, "La firma es requerida para finalizar el servicio.", null);

        if (string.IsNullOrWhiteSpace(dto.NombreQuienFirma))
            return (false, "El nombre de quien firma es requerido.", null);

        var totalFotos = await _context.ServiciosFotos.CountAsync(f => f.ServicioId == servicio.Id);
        if (totalFotos < 3)
            return (false, "Se requieren al menos 3 fotos de evidencia antes de finalizar el servicio.", null);

        servicio.FirmaBase64      = dto.FirmaBase64;
        servicio.NombreQuienFirma = dto.NombreQuienFirma;
        servicio.FechaFirma       = DateTime.UtcNow;
        servicio.FechaFin         = DateTime.UtcNow;
        servicio.Estado           = EstadoServicio.Terminado;

        // Candado de un solo uso: la liga deja de ser válida en cuanto se firma.
        servicio.TokenPublico = null;
        servicio.TokenExpira = null;

        await _context.SaveChangesAsync();

        var result = await _context.Servicios.AsNoTracking().Include(s => s.Cliente).FirstAsync(s => s.Id == servicio.Id);

        await _bitacora.RegistrarAsync(servicio.OperadorId, servicio.OperadorNombre, "Firmó", "Servicio", $"Servicio #{servicio.Id} finalizado y firmado por {dto.NombreQuienFirma} (vía liga pública)");

        return (true, "Servicio finalizado y firmado correctamente.", ServicioPublicoDto.FromEntity(result));
    }
}
