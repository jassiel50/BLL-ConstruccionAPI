using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.DTOs.GastosExtras;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Services;

public class GastoExtraService : IGastoExtraService
{
    private readonly AppDbContext _context;

    public GastoExtraService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<GastoExtraDto>> GetByFaseAsync(int faseId)
    {
        return await _context.GastosExtras
            .Where(g => g.FaseId == faseId)
            .Select(g => new GastoExtraDto
            {
                Id = g.Id,
                FaseId = g.FaseId,
                Concepto = g.Concepto,
                Monto = g.Monto,
                Fecha = g.Fecha,
                Observaciones = g.Observaciones
            })
            .ToListAsync();
    }

    public async Task<(bool Success, GastoExtraDto? Data)> CreateAsync(int faseId, GastoExtraRequestDto dto)
    {
        var faseExists = await _context.FaseProyectos.AnyAsync(f => f.Id == faseId);
        if (!faseExists)
            return (false, null);

        var entity = new GastoExtra
        {
            FaseId = faseId,
            Concepto = dto.Concepto,
            Monto = dto.Monto,
            Fecha = dto.Fecha,
            Observaciones = dto.Observaciones,
            FechaRegistro = DateTime.UtcNow
        };

        _context.GastosExtras.Add(entity);
        await _context.SaveChangesAsync();

        return (true, new GastoExtraDto
        {
            Id = entity.Id,
            FaseId = entity.FaseId,
            Concepto = entity.Concepto,
            Monto = entity.Monto,
            Fecha = entity.Fecha,
            Observaciones = entity.Observaciones
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.GastosExtras.FindAsync(id);
        if (entity is null) return false;

        _context.GastosExtras.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
