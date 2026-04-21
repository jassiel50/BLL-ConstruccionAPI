using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Models.Enums;
using BLL_ConstruccionAPI.Models.Inventario.Herramientas;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;
using BLL_ConstruccionAPI.Models.Inventario.Proyectos;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Repositories;

public class ProyectosRepository : IProyectosRepository
{
    private readonly AppDbContext _context;

    public ProyectosRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Proyecto>> GetAllAsync()
        => await _context.Proyectos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Where(p => p.Activo)
            .ToListAsync();

    public async Task<IEnumerable<Proyecto>> GetByClienteAsync(int clienteId)
        => await _context.Proyectos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Where(p => p.ClienteId == clienteId && p.Activo)
            .ToListAsync();

    public async Task<Proyecto?> GetByIdAsync(int id)
        => await _context.Proyectos
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<int> CreateAsync(Proyecto proyecto)
    {
        _context.Proyectos.Add(proyecto);
        await _context.SaveChangesAsync();
        return proyecto.Id;
    }

    public async Task UpdateAsync(Proyecto proyecto)
    {
        _context.Proyectos.Update(proyecto);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Proyecto proyecto)
    {
        proyecto.Activo = false;
        _context.Proyectos.Update(proyecto);
        await _context.SaveChangesAsync();
    }

    public async Task TerminarAsync(Proyecto proyecto)
    {
        proyecto.Estado = EstadoProyecto.Terminado;
        _context.Proyectos.Update(proyecto);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AlmacenProyecto>> GetMaterialesAsync(int proyectoId)
        => await _context.AlmacenProyecto
            .AsNoTracking()
            .Include(ap => ap.Material)
                .ThenInclude(m => m!.UnidadMedida)
            .Where(ap => ap.ProyectoId == proyectoId)
            .ToListAsync();

    public async Task<IEnumerable<AsignacionHerramienta>> GetHerramientasActivasAsync(int proyectoId)
        => await _context.AsignacionesHerramienta
            .AsNoTracking()
            .Include(a => a.Herramienta)
            .Where(a => a.ProyectoId == proyectoId && a.Estado == EstadoAsignacion.Asignada)
            .OrderByDescending(a => a.FechaAsignacion)
            .ToListAsync();
}
