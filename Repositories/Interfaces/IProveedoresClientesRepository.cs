using BLL_ConstruccionAPI.Models.Inventario;

namespace BLL_ConstruccionAPI.Repositories.Interfaces;

public interface IProveedoresClientesRepository
{
    // ─── Proveedores ──────────────────────────────────────────────────────────
    Task<IEnumerable<Proveedor>> GetAllProveedoresAsync();
    Task<Proveedor?> GetProveedorByIdAsync(int id);
    Task<bool> ExisteProveedorRFCAsync(string rfc);
    Task<int> CreateProveedorAsync(Proveedor proveedor);
    Task UpdateProveedorAsync(Proveedor proveedor);
    Task DeleteProveedorAsync(Proveedor proveedor);

    // ─── Clientes ─────────────────────────────────────────────────────────────
    Task<IEnumerable<Cliente>> GetAllClientesAsync();
    Task<Cliente?> GetClienteByIdAsync(int id);
    Task<bool> ExisteClienteRFCAsync(string rfc);
    Task<int> CreateClienteAsync(Cliente cliente);
    Task UpdateClienteAsync(Cliente cliente);
    Task DeleteClienteAsync(Cliente cliente);
}
