using BLL_ConstruccionAPI.DTOs.ProveedoresClientes;

namespace BLL_ConstruccionAPI.Services.Interfaces;

public interface IProveedoresClientesService
{
    // ─── Proveedores ──────────────────────────────────────────────────────────
    Task<IEnumerable<ProveedorResponseDto>> GetAllProveedoresAsync();
    Task<ProveedorResponseDto?> GetProveedorByIdAsync(int id);
    Task<(bool Success, string Message, ProveedorResponseDto? Data)> CreateProveedorAsync(ProveedorRequestDto dto);
    Task<(bool Success, string Message)> UpdateProveedorAsync(int id, ProveedorRequestDto dto);
    Task<(bool Success, string Message)> DeleteProveedorAsync(int id);

    // ─── Contactos Proveedor ──────────────────────────────────────────────────
    Task<(bool Success, string Message, ContactoResponseDto? Data)> AddContactoProveedorAsync(int proveedorId, ContactoRequestDto dto);
    Task<(bool Success, string Message)> UpdateContactoProveedorAsync(int proveedorId, int contactoId, ContactoRequestDto dto);
    Task<(bool Success, string Message)> DeleteContactoProveedorAsync(int proveedorId, int contactoId);

    // ─── Clientes ─────────────────────────────────────────────────────────────
    Task<IEnumerable<ClienteResponseDto>> GetAllClientesAsync();
    Task<ClienteResponseDto?> GetClienteByIdAsync(int id);
    Task<(bool Success, string Message, ClienteResponseDto? Data)> CreateClienteAsync(ClienteRequestDto dto);
    Task<(bool Success, string Message)> UpdateClienteAsync(int id, ClienteRequestDto dto);
    Task<(bool Success, string Message)> DeleteClienteAsync(int id);

    // ─── Contactos Cliente ────────────────────────────────────────────────────
    Task<(bool Success, string Message, ContactoResponseDto? Data)> AddContactoClienteAsync(int clienteId, ContactoRequestDto dto);
    Task<(bool Success, string Message)> UpdateContactoClienteAsync(int clienteId, int contactoId, ContactoRequestDto dto);
    Task<(bool Success, string Message)> DeleteContactoClienteAsync(int clienteId, int contactoId);
}
