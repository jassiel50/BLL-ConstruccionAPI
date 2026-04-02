using BLL_ConstruccionAPI.DTOs.ProveedoresClientes;
using BLL_ConstruccionAPI.Models.Inventario;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IProveedoresClientesService
{
    // ─── Proveedores ──────────────────────────────────────────────────────────
    Task<IEnumerable<Proveedor>> GetAllProveedoresAsync();
    Task<Proveedor?> GetProveedorByIdAsync(int id);
    Task<(bool Success, string Message, Proveedor? Data)> CreateProveedorAsync(ProveedorRequestDto dto);
    Task<(bool Success, string Message)> UpdateProveedorAsync(int id, ProveedorRequestDto dto);
    Task<(bool Success, string Message)> DeleteProveedorAsync(int id);

    // ─── Clientes ─────────────────────────────────────────────────────────────
    Task<IEnumerable<Cliente>> GetAllClientesAsync();
    Task<Cliente?> GetClienteByIdAsync(int id);
    Task<(bool Success, string Message, Cliente? Data)> CreateClienteAsync(ClienteRequestDto dto);
    Task<(bool Success, string Message)> UpdateClienteAsync(int id, ClienteRequestDto dto);
    Task<(bool Success, string Message)> DeleteClienteAsync(int id);
}
