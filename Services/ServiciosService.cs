using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Servicios;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Servicios;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class ServiciosService : IServiciosService
{
    private const int RolOperadorServicio = 4;

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
        if (!Enum.TryParse<TipoServicio>(dto.Tipo, ignoreCase: true, out var tipo))
            return (false, $"Tipo de servicio inválido: '{dto.Tipo}'.", null);

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
            FechaCreacion        = DateTime.UtcNow
        };

        _context.Servicios.Add(servicio);
        await _context.SaveChangesAsync();

        var result = await _context.Servicios
            .AsNoTracking()
            .Include(s => s.Cliente)
            .FirstAsync(s => s.Id == servicio.Id);

        var nombreCliente = result.ClienteId.HasValue && result.Cliente is not null ? result.Cliente.Nombre : result.ClienteNombre;
        await _bitacora.RegistrarAsync(usuarioId, nombreUsuario, "Registró", "Servicio", $"Nuevo servicio de {tipo} para '{nombreCliente}'");

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

        if (!Enum.TryParse<TipoServicio>(dto.Tipo, ignoreCase: true, out var tipo))
            return (false, $"Tipo de servicio inválido: '{dto.Tipo}'.", null);

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
}
