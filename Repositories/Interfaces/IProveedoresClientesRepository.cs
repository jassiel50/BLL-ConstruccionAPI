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

    // ─── Contactos Proveedor ──────────────────────────────────────────────────
    Task<ContactoProveedor?> GetContactoProveedorAsync(int id);
    Task AddContactoProveedorAsync(ContactoProveedor contacto);
    Task UpdateContactoProveedorAsync(ContactoProveedor contacto);
    Task DeleteContactoProveedorAsync(ContactoProveedor contacto);

    // ─── Clientes ─────────────────────────────────────────────────────────────
    Task<IEnumerable<Cliente>> GetAllClientesAsync();
    Task<Cliente?> GetClienteByIdAsync(int id);
    Task<bool> ExisteClienteRFCAsync(string rfc);
    Task<int> CreateClienteAsync(Cliente cliente);
    Task UpdateClienteAsync(Cliente cliente);
    Task DeleteClienteAsync(Cliente cliente);

    // ─── Contactos Cliente ────────────────────────────────────────────────────
    Task<ContactoCliente?> GetContactoClienteAsync(int id);
    Task AddContactoClienteAsync(ContactoCliente contacto);
    Task UpdateContactoClienteAsync(ContactoCliente contacto);
    Task DeleteContactoClienteAsync(ContactoCliente contacto);
}
