using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Repositories;

public class MaterialesRepository : IMaterialesRepository
{
    private readonly AppDbContext _context;

    public MaterialesRepository(AppDbContext context)
    {
        _context = context;
    }

    // ─── Material ─────────────────────────────────────────────────────────────
    public async Task<IEnumerable<Material>> GetAllAsync()
        => await _context.Materiales
            .AsNoTracking()
            .Include(m => m.Categoria)
            .Include(m => m.UnidadMedida)
            .Where(m => m.Activo)
            .ToListAsync();

    public async Task<IEnumerable<Material>> GetBajoStockAsync()
        => await _context.Materiales
            .AsNoTracking()
            .Include(m => m.Categoria)
            .Include(m => m.UnidadMedida)
            .Where(m => m.Activo &&
                _context.AlmacenCentral.Any(ac => ac.MaterialId == m.Id && ac.Stock < m.StockMinimo))
            .ToListAsync();

    public async Task<Material?> GetByIdAsync(int id)
        => await _context.Materiales
            .Include(m => m.Categoria)
            .Include(m => m.UnidadMedida)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<bool> ExisteCodigoAsync(string codigo)
        => await _context.Materiales.AnyAsync(m => m.Codigo == codigo && m.Activo);

    public async Task<int> CreateAsync(Material material)
    {
        _context.Materiales.Add(material);
        await _context.SaveChangesAsync();
        return material.Id;
    }

    public async Task UpdateAsync(Material material)
    {
        _context.Materiales.Update(material);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Material material)
    {
        material.Activo = false;
        _context.Materiales.Update(material);
        await _context.SaveChangesAsync();
    }

    // ─── AlmacenCentral ───────────────────────────────────────────────────────
    public async Task<IEnumerable<AlmacenCentral>> GetAllStockCentralAsync()
        => await _context.AlmacenCentral
            .AsNoTracking()
            .Include(ac => ac.Material)
            .ToListAsync();

    public async Task<AlmacenCentral?> GetStockCentralAsync(int materialId)
        => await _context.AlmacenCentral
            .Include(ac => ac.Material)
            .FirstOrDefaultAsync(ac => ac.MaterialId == materialId);

    public async Task CreateStockCentralAsync(AlmacenCentral almacen)
    {
        _context.AlmacenCentral.Add(almacen);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStockCentralAsync(AlmacenCentral almacen)
    {
        almacen.UltimaActualizacion = DateTime.UtcNow;
        _context.AlmacenCentral.Update(almacen);
        await _context.SaveChangesAsync();
    }
}
