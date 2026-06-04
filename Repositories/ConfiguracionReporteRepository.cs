using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Models.Reportes;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Repositories;

public class ConfiguracionReporteRepository : IConfiguracionReporteRepository
{
    private readonly AppDbContext _context;

    public ConfiguracionReporteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ConfiguracionReporte>> GetByUsuarioIdAsync(int usuarioId) =>
        await _context.ConfiguracionesReporte
            .Where(c => c.UsuarioId == usuarioId)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

    public async Task<ConfiguracionReporte?> GetByIdAsync(int id) =>
        await _context.ConfiguracionesReporte.FindAsync(id);

    public async Task<ConfiguracionReporte> CreateAsync(ConfiguracionReporte config)
    {
        _context.ConfiguracionesReporte.Add(config);
        await _context.SaveChangesAsync();
        return config;
    }

    public async Task UpdateAsync(ConfiguracionReporte config)
    {
        _context.ConfiguracionesReporte.Update(config);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ConfiguracionReporte config)
    {
        _context.ConfiguracionesReporte.Remove(config);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ConfiguracionReporte>> GetActivasAsync() =>
        await _context.ConfiguracionesReporte
            .Include(c => c.Usuario)
            .Where(c => c.Activo)
            .ToListAsync();
}
