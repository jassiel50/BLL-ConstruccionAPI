using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Models.Inventario.Materiales;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Repositories;

public class EntradasRepository : IEntradasRepository
{
    private readonly AppDbContext _context;

    public EntradasRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Entrada>> GetAllAsync()
        => await _context.Entradas
            .AsNoTracking()
            .Include(e => e.Proveedor)
            .Include(e => e.Detalles)
                .ThenInclude(d => d.Material)
            .OrderByDescending(e => e.Fecha)
            .ToListAsync();

    public async Task<Entrada?> GetByIdAsync(int id)
        => await _context.Entradas
            .Include(e => e.Proveedor)
            .Include(e => e.Detalles)
                .ThenInclude(d => d.Material)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<bool> ExisteFolioAsync(string numeroFolio)
        => await _context.Entradas.AnyAsync(e => e.NumeroFolio == numeroFolio);

    // Agrega la Entrada (con sus Detalles ya incluidos en la colección)
    // y guarda TODO lo que el Change Tracker tenga pendiente en una sola transacción:
    //   → EntradasDetalle (en cascada por la colección)
    //   → AlmacenCentral (modificado en memoria por el servicio antes de esta llamada)
    public async Task<int> RegistrarEntradaAsync(Entrada entrada)
    {
        _context.Entradas.Add(entrada);
        await _context.SaveChangesAsync();
        return entrada.Id;
    }
}
