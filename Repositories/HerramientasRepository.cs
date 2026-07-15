using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Herramientas;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Repositories;

public class HerramientasRepository : IHerramientasRepository
{
    private readonly AppDbContext _context;

    public HerramientasRepository(AppDbContext context)
    {
        _context = context;
    }

    // ─── Herramienta ─────────────────────────────────────────────────────────
    public async Task<IEnumerable<Herramienta>> GetAllAsync()
        => await _context.Herramientas
            .AsNoTracking()
            .Include(h => h.CategoriaHerramienta)
            .Where(h => h.Activo)
            .ToListAsync();

    public async Task<IEnumerable<Herramienta>> GetDisponiblesAsync()
        => await _context.Herramientas
            .AsNoTracking()
            .Include(h => h.CategoriaHerramienta)
            .Where(h => h.Activo && h.Estado == EstadoHerramienta.Disponible)
            .ToListAsync();

    public async Task<Herramienta?> GetByIdAsync(int id)
        => await _context.Herramientas
            .Include(h => h.CategoriaHerramienta)
            .FirstOrDefaultAsync(h => h.Id == id);

    public async Task<bool> ExisteCodigoAsync(string codigo)
        => await _context.Herramientas.AnyAsync(h => h.Codigo == codigo && h.Activo);

    public async Task<bool> ExisteNumeroSerieAsync(string numeroSerie)
        => await _context.Herramientas.AnyAsync(h => h.NumeroSerie == numeroSerie && h.Activo);

    public async Task<int> CreateAsync(Herramienta herramienta)
    {
        _context.Herramientas.Add(herramienta);
        await _context.SaveChangesAsync();
        return herramienta.Id;
    }

    public async Task UpdateAsync(Herramienta herramienta)
    {
        _context.Herramientas.Update(herramienta);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Herramienta herramienta)
    {
        herramienta.Activo = false;
        herramienta.Estado = EstadoHerramienta.Baja;
        _context.Herramientas.Update(herramienta);
        await _context.SaveChangesAsync();
    }

    // ─── Asignaciones ─────────────────────────────────────────────────────────
    public async Task<IEnumerable<AsignacionHerramienta>> GetAsignacionesByHerramientaAsync(int herramientaId)
        => await _context.AsignacionesHerramienta
            .AsNoTracking()
            .Include(a => a.Proyecto)
            .Where(a => a.HerramientaId == herramientaId)
            .OrderByDescending(a => a.FechaAsignacion)
            .ToListAsync();

    public async Task<IEnumerable<AsignacionHerramienta>> GetAsignacionesActivasAsync()
        => await _context.AsignacionesHerramienta
            .AsNoTracking()
            .Include(a => a.Herramienta)
            .Include(a => a.Proyecto)
            .Where(a => a.Estado == EstadoAsignacion.Asignada)
            .OrderByDescending(a => a.FechaAsignacion)
            .ToListAsync();

    public async Task<AsignacionHerramienta?> GetAsignacionByIdAsync(int id)
        => await _context.AsignacionesHerramienta
            .Include(a => a.Herramienta)
            .Include(a => a.Proyecto)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<AsignacionHerramienta?> GetAsignacionActivaAsync(int herramientaId)
        => await _context.AsignacionesHerramienta
            .FirstOrDefaultAsync(a => a.HerramientaId == herramientaId && a.Estado == EstadoAsignacion.Asignada);

    public async Task<int> GetCantidadAsignadaAsync(int herramientaId)
        => await _context.AsignacionesHerramienta
            .CountAsync(a => a.HerramientaId == herramientaId && a.Estado == EstadoAsignacion.Asignada);

    public async Task<int> CreateAsignacionAsync(AsignacionHerramienta asignacion)
    {
        _context.AsignacionesHerramienta.Add(asignacion);
        await _context.SaveChangesAsync();
        return asignacion.Id;
    }

    public async Task UpdateAsignacionAsync(AsignacionHerramienta asignacion)
    {
        _context.AsignacionesHerramienta.Update(asignacion);
        await _context.SaveChangesAsync();
    }

    public async Task AsignarHerramientaAsync(AsignacionHerramienta asignacion, Herramienta herramienta)
    {
        _context.AsignacionesHerramienta.Add(asignacion);
        _context.Herramientas.Update(herramienta);
        await _context.SaveChangesAsync();
    }

    public async Task DevolverHerramientaAsync(AsignacionHerramienta asignacion, Herramienta herramienta)
    {
        _context.AsignacionesHerramienta.Update(asignacion);
        _context.Herramientas.Update(herramienta);
        await _context.SaveChangesAsync();
    }

    public async Task TransferirHerramientaAsync(AsignacionHerramienta asignacionActual, AsignacionHerramienta nuevaAsignacion)
    {
        _context.AsignacionesHerramienta.Update(asignacionActual);
        _context.AsignacionesHerramienta.Add(nuevaAsignacion);
        await _context.SaveChangesAsync();
    }
}
