using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Models.Inventario.Cátalogos;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Repositories;

public class CatalogosRepository : ICatalogosRepository
{
    private readonly AppDbContext _context;

    public CatalogosRepository(AppDbContext context)
    {
        _context = context;
    }

    // ─── CategoriaMaterial ────────────────────────────────────────────────────
    public async Task<IEnumerable<CategoriaMaterial>> GetAllCategoriasAsync()
        => await _context.Categorias.Where(c => c.Activo).ToListAsync();

    public async Task<CategoriaMaterial?> GetCategoriaByIdAsync(int id)
        => await _context.Categorias.FindAsync(id);

    public async Task<bool> ExisteCategoriaAsync(string nombre)
        => await _context.Categorias.AnyAsync(c => c.Nombre == nombre && c.Activo);

    public async Task<int> CreateCategoriaAsync(CategoriaMaterial categoria)
    {
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return categoria.Id;
    }

    public async Task UpdateCategoriaAsync(CategoriaMaterial categoria)
    {
        _context.Categorias.Update(categoria);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoriaAsync(CategoriaMaterial categoria)
    {
        categoria.Activo = false;
        _context.Categorias.Update(categoria);
        await _context.SaveChangesAsync();
    }

    // ─── CategoriaHerramienta ─────────────────────────────────────────────────
    public async Task<IEnumerable<CategoriaHerramienta>> GetAllCategoriasHerramientaAsync()
        => await _context.CategoriasHerramienta.Where(c => c.Activo).ToListAsync();

    public async Task<CategoriaHerramienta?> GetCategoriaHerramientaByIdAsync(int id)
        => await _context.CategoriasHerramienta.FindAsync(id);

    public async Task<bool> ExisteCategoriaHerramientaAsync(string nombre)
        => await _context.CategoriasHerramienta.AnyAsync(c => c.Nombre == nombre && c.Activo);

    public async Task<int> CreateCategoriaHerramientaAsync(CategoriaHerramienta categoria)
    {
        _context.CategoriasHerramienta.Add(categoria);
        await _context.SaveChangesAsync();
        return categoria.Id;
    }

    public async Task UpdateCategoriaHerramientaAsync(CategoriaHerramienta categoria)
    {
        _context.CategoriasHerramienta.Update(categoria);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoriaHerramientaAsync(CategoriaHerramienta categoria)
    {
        categoria.Activo = false;
        _context.CategoriasHerramienta.Update(categoria);
        await _context.SaveChangesAsync();
    }

    // ─── UnidadMedida ─────────────────────────────────────────────────────────
    public async Task<IEnumerable<UnidadMedida>> GetAllUnidadesAsync()
        => await _context.UnidadesMedida.Where(u => u.Activo).ToListAsync();

    public async Task<UnidadMedida?> GetUnidadByIdAsync(int id)
        => await _context.UnidadesMedida.FindAsync(id);

    public async Task<bool> ExisteUnidadAsync(string abreviatura)
        => await _context.UnidadesMedida.AnyAsync(u => u.Abreviatura == abreviatura && u.Activo);

    public async Task<int> CreateUnidadAsync(UnidadMedida unidad)
    {
        _context.UnidadesMedida.Add(unidad);
        await _context.SaveChangesAsync();
        return unidad.Id;
    }

    public async Task UpdateUnidadAsync(UnidadMedida unidad)
    {
        _context.UnidadesMedida.Update(unidad);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUnidadAsync(UnidadMedida unidad)
    {
        unidad.Activo = false;
        _context.UnidadesMedida.Update(unidad);
        await _context.SaveChangesAsync();
    }
}
