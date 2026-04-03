using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Models.Inventario;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Repositories;

public class ProveedoresClientesRepository : IProveedoresClientesRepository
{
    private readonly AppDbContext _context;

    public ProveedoresClientesRepository(AppDbContext context)
    {
        _context = context;
    }

    // ─── Proveedores ──────────────────────────────────────────────────────────
    public async Task<IEnumerable<Proveedor>> GetAllProveedoresAsync()
        => await _context.Proveedores.AsNoTracking().Where(p => p.Activo).ToListAsync();

    public async Task<Proveedor?> GetProveedorByIdAsync(int id)
        => await _context.Proveedores.FindAsync(id);

    public async Task<bool> ExisteProveedorRFCAsync(string rfc)
        => await _context.Proveedores.AnyAsync(p => p.RFC == rfc && p.Activo);

    public async Task<int> CreateProveedorAsync(Proveedor proveedor)
    {
        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();
        return proveedor.Id;
    }

    public async Task UpdateProveedorAsync(Proveedor proveedor)
    {
        _context.Proveedores.Update(proveedor);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProveedorAsync(Proveedor proveedor)
    {
        proveedor.Activo = false;
        _context.Proveedores.Update(proveedor);
        await _context.SaveChangesAsync();
    }

    // ─── Clientes ─────────────────────────────────────────────────────────────
    public async Task<IEnumerable<Cliente>> GetAllClientesAsync()
        => await _context.Clientes.AsNoTracking().Where(c => c.Activo).ToListAsync();

    public async Task<Cliente?> GetClienteByIdAsync(int id)
        => await _context.Clientes.FindAsync(id);

    public async Task<bool> ExisteClienteRFCAsync(string rfc)
        => await _context.Clientes.AnyAsync(c => c.RFC == rfc && c.Activo);

    public async Task<int> CreateClienteAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente.Id;
    }

    public async Task UpdateClienteAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteClienteAsync(Cliente cliente)
    {
        cliente.Activo = false;
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
    }
}
