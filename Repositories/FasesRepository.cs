using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Repositories;

public class FasesRepository : IFasesRepository
{
    private readonly AppDbContext _context;

    public FasesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FaseProyecto>> GetByProyectoAsync(int proyectoId)
        => await _context.FaseProyectos
            .AsNoTracking()
            .Where(f => f.ProyectoId == proyectoId)
            .OrderBy(f => f.Orden)
            .ToListAsync();

    public async Task<FaseProyecto?> GetByIdAsync(int id)
        => await _context.FaseProyectos
            .FirstOrDefaultAsync(f => f.Id == id);

    public async Task<List<FaseProyecto>> GetAtrasadasAsync()
        => await _context.FaseProyectos
            .AsNoTracking()
            .Include(f => f.Proyecto)
            .Where(f => f.Estado != EstadoFase.Completada && f.FechaLimite < DateTime.UtcNow)
            .OrderBy(f => f.FechaLimite)
            .ToListAsync();

    public async Task<List<FaseProyecto>> GetPorVencerAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var limite = hoy.AddDays(2);
        return await _context.FaseProyectos
            .AsNoTracking()
            .Include(f => f.Proyecto)
            .Where(f => f.Estado != EstadoFase.Completada
                     && f.FechaLimite.Date >= hoy
                     && f.FechaLimite.Date <= limite)
            .OrderBy(f => f.FechaLimite)
            .ToListAsync();
    }

    public async Task<int> CreateAsync(FaseProyecto fase)
    {
        _context.FaseProyectos.Add(fase);
        await _context.SaveChangesAsync();
        return fase.Id;
    }

    public async Task UpdateAsync(FaseProyecto fase)
    {
        _context.FaseProyectos.Update(fase);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(FaseProyecto fase)
    {
        _context.FaseProyectos.Remove(fase);
        await _context.SaveChangesAsync();
    }
}
