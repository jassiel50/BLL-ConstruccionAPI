using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Repositories;

public class SalidasRepository : ISalidasRepository
{
    private readonly AppDbContext _context;

    public SalidasRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Salida>> GetAllAsync()
        => await _context.Salidas
            .AsNoTracking()
            .Include(s => s.Proyecto)
            .Include(s => s.Detalles)
                .ThenInclude(d => d.Material)
            .OrderByDescending(s => s.Fecha)
            .ToListAsync();

    public async Task<IEnumerable<Salida>> GetByProyectoAsync(int proyectoId)
        => await _context.Salidas
            .AsNoTracking()
            .Include(s => s.Proyecto)
            .Include(s => s.Detalles)
                .ThenInclude(d => d.Material)
            .Where(s => s.ProyectoId == proyectoId)
            .OrderByDescending(s => s.Fecha)
            .ToListAsync();

    public async Task<Salida?> GetByIdAsync(int id)
        => await _context.Salidas
            .Include(s => s.Proyecto)
            .Include(s => s.Detalles)
                .ThenInclude(d => d.Material)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<bool> ExisteFolioAsync(string numeroFolio)
        => await _context.Salidas.AnyAsync(s => s.NumeroFolio == numeroFolio);

    public async Task<AlmacenProyecto?> GetStockProyectoAsync(int proyectoId, int materialId)
        => await _context.AlmacenProyecto
            .FirstOrDefaultAsync(ap => ap.ProyectoId == proyectoId && ap.MaterialId == materialId);

    public async Task<IEnumerable<AlmacenProyecto>> GetAlmacenProyectoAsync(int proyectoId)
        => await _context.AlmacenProyecto
            .AsNoTracking()
            .Include(ap => ap.Material)
            .Where(ap => ap.ProyectoId == proyectoId)
            .ToListAsync();

    // Solo añade al Change Tracker, sin SaveChangesAsync
    // Se guardará junto con la Salida en RegistrarSalidaAsync
    public void TrackNuevoStockProyecto(AlmacenProyecto almacen)
        => _context.AlmacenProyecto.Add(almacen);

    // Guarda la Salida (con sus Detalles en cascada) + AlmacenCentral modificado
    // + AlmacenProyecto nuevo o modificado → todo en una sola transacción
    public async Task<int> RegistrarSalidaAsync(Salida salida)
    {
        _context.Salidas.Add(salida);
        await _context.SaveChangesAsync();
        return salida.Id;
    }
}
