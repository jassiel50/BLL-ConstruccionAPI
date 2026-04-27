using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Devoluciones;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class DevolucionesMaterialService : IDevolucionesMaterialService
{
    private readonly AppDbContext _context;

    public DevolucionesMaterialService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DevolucionMaterialResponseDto>> GetAllAsync()
    {
        var devoluciones = await _context.DevolucionesMaterial
            .AsNoTracking()
            .Include(d => d.Material)
            .Include(d => d.Proyecto)
            .OrderByDescending(d => d.FechaDevolucion)
            .ToListAsync();

        return devoluciones.Select(DevolucionMaterialResponseDto.FromEntity);
    }

    public async Task<IEnumerable<DevolucionMaterialResponseDto>> GetByProyectoAsync(int proyectoId)
    {
        var devoluciones = await _context.DevolucionesMaterial
            .AsNoTracking()
            .Include(d => d.Material)
            .Include(d => d.Proyecto)
            .Where(d => d.ProyectoId == proyectoId)
            .OrderByDescending(d => d.FechaDevolucion)
            .ToListAsync();

        return devoluciones.Select(DevolucionMaterialResponseDto.FromEntity);
    }

    public async Task<DevolucionMaterialResponseDto?> GetByIdAsync(int id)
    {
        var devolucion = await _context.DevolucionesMaterial
            .AsNoTracking()
            .Include(d => d.Material)
            .Include(d => d.Proyecto)
            .FirstOrDefaultAsync(d => d.Id == id);

        return devolucion is null ? null : DevolucionMaterialResponseDto.FromEntity(devolucion);
    }

    public async Task<(bool Success, string Message, DevolucionMaterialResponseDto? Data)> CreateAsync(int usuarioId, DevolucionMaterialRequestDto dto)
    {
        if (dto.CantidadDevuelta <= 0)
            return (false, "La cantidad a devolver debe ser mayor a cero.", null);

        // Verificar stock en AlmacenProyecto
        var stockProyecto = await _context.AlmacenProyecto
            .FirstOrDefaultAsync(ap => ap.ProyectoId == dto.ProyectoId && ap.MaterialId == dto.MaterialId);

        if (stockProyecto is null)
            return (false, "Este material no está asignado a este proyecto.", null);

        if (stockProyecto.Stock < dto.CantidadDevuelta)
            return (false, $"La cantidad a devolver ({dto.CantidadDevuelta}) supera el stock disponible en el proyecto ({stockProyecto.Stock}).", null);

        // Restar del AlmacenProyecto
        stockProyecto.Stock -= dto.CantidadDevuelta;
        stockProyecto.UltimaActualizacion = DateTime.UtcNow;

        // Sumar al AlmacenCentral (o crear si no existe)
        var stockCentral = await _context.AlmacenCentral
            .FirstOrDefaultAsync(ac => ac.MaterialId == dto.MaterialId);

        if (stockCentral is not null)
        {
            stockCentral.Stock += dto.CantidadDevuelta;
            stockCentral.UltimaActualizacion = DateTime.UtcNow;
        }
        else
        {
            _context.AlmacenCentral.Add(new AlmacenCentral
            {
                MaterialId          = dto.MaterialId,
                Stock               = dto.CantidadDevuelta,
                Zona                = stockProyecto.Zona,
                TipoUbicacion       = TipoUbicacion.Almacen,
                UltimaActualizacion = DateTime.UtcNow
            });
        }

        // Registrar la devolución
        var devolucion = new DevolucionMaterial
        {
            ProyectoId       = dto.ProyectoId,
            MaterialId       = dto.MaterialId,
            UsuarioId        = usuarioId,
            CantidadDevuelta = dto.CantidadDevuelta,
            Observaciones    = dto.Observaciones,
            FechaDevolucion  = DateTime.UtcNow
        };

        _context.DevolucionesMaterial.Add(devolucion);

        // Único SaveChangesAsync: AlmacenProyecto - stock + AlmacenCentral + stock + DevolucionMaterial
        await _context.SaveChangesAsync();

        // Recargar con navegación para el response
        var result = await _context.DevolucionesMaterial
            .AsNoTracking()
            .Include(d => d.Material)
            .Include(d => d.Proyecto)
            .FirstAsync(d => d.Id == devolucion.Id);

        return (true, "Devolución de material registrada correctamente.", DevolucionMaterialResponseDto.FromEntity(result));
    }
}
