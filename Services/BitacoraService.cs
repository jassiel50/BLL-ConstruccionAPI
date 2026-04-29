using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.Bitacora;
using BLL_ConstruccionAPI.Models.Auth;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class BitacoraService : IBitacoraService
{
    private readonly AppDbContext _context;

    public BitacoraService(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(int usuarioId, string nombreUsuario, string accion, string entidad, string descripcion, string? ip = null)
    {
        var entrada = new BitacoraActividad
        {
            UsuarioId     = usuarioId,
            NombreUsuario = nombreUsuario,
            Accion        = accion,
            Entidad       = entidad,
            Descripcion   = descripcion,
            Fecha         = DateTime.UtcNow,
            IpOrigen      = ip
        };

        _context.Bitacora.Add(entrada);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<BitacoraActividadDto>> GetAllAsync()
    {
        var registros = await _context.Bitacora
            .AsNoTracking()
            .OrderByDescending(b => b.Fecha)
            .ToListAsync();

        return registros.Select(BitacoraActividadDto.FromEntity);
    }

    public async Task<IEnumerable<BitacoraActividadDto>> GetByUsuarioAsync(int usuarioId)
    {
        var registros = await _context.Bitacora
            .AsNoTracking()
            .Where(b => b.UsuarioId == usuarioId)
            .OrderByDescending(b => b.Fecha)
            .ToListAsync();

        return registros.Select(BitacoraActividadDto.FromEntity);
    }
}
