using BLL_ConstruccionAPI.DTOs.ProveedoresClientes;
using BLL_ConstruccionAPI.Models.Inventario;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;

namespace BLL_ConstruccionAPI.Services;

public class ProveedoresClientesService : IProveedoresClientesService
{
    private readonly IProveedoresClientesRepository _repo;

    public ProveedoresClientesService(IProveedoresClientesRepository repo)
    {
        _repo = repo;
    }

    // ─── Proveedores ──────────────────────────────────────────────────────────
    public async Task<IEnumerable<Proveedor>> GetAllProveedoresAsync()
        => await _repo.GetAllProveedoresAsync();

    public async Task<Proveedor?> GetProveedorByIdAsync(int id)
        => await _repo.GetProveedorByIdAsync(id);

    public async Task<(bool Success, string Message, Proveedor? Data)> CreateProveedorAsync(ProveedorRequestDto dto)
    {
        if (await _repo.ExisteProveedorRFCAsync(dto.RFC))
            return (false, "Ya existe un proveedor con ese RFC.", null);

        var proveedor = new Proveedor
        {
            Nombre = dto.Nombre,
            RFC = dto.RFC,
            Contacto = dto.Contacto,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Direccion = dto.Direccion,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        await _repo.CreateProveedorAsync(proveedor);
        return (true, "Proveedor registrado correctamente.", proveedor);
    }

    public async Task<(bool Success, string Message)> UpdateProveedorAsync(int id, ProveedorRequestDto dto)
    {
        var proveedor = await _repo.GetProveedorByIdAsync(id);
        if (proveedor is null) return (false, "Proveedor no encontrado.");

        if (proveedor.RFC != dto.RFC && await _repo.ExisteProveedorRFCAsync(dto.RFC))
            return (false, "Ya existe un proveedor con ese RFC.");

        proveedor.Nombre = dto.Nombre;
        proveedor.RFC = dto.RFC;
        proveedor.Contacto = dto.Contacto;
        proveedor.Telefono = dto.Telefono;
        proveedor.Email = dto.Email;
        proveedor.Direccion = dto.Direccion;

        await _repo.UpdateProveedorAsync(proveedor);
        return (true, "Proveedor actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteProveedorAsync(int id)
    {
        var proveedor = await _repo.GetProveedorByIdAsync(id);
        if (proveedor is null) return (false, "Proveedor no encontrado.");

        await _repo.DeleteProveedorAsync(proveedor);
        return (true, "Proveedor desactivado correctamente.");
    }

    // ─── Clientes ─────────────────────────────────────────────────────────────
    public async Task<IEnumerable<Cliente>> GetAllClientesAsync()
        => await _repo.GetAllClientesAsync();

    public async Task<Cliente?> GetClienteByIdAsync(int id)
        => await _repo.GetClienteByIdAsync(id);

    public async Task<(bool Success, string Message, Cliente? Data)> CreateClienteAsync(ClienteRequestDto dto)
    {
        if (await _repo.ExisteClienteRFCAsync(dto.RFC))
            return (false, "Ya existe un cliente con ese RFC.", null);

        var cliente = new Cliente
        {
            Nombre = dto.Nombre,
            RFC = dto.RFC,
            Contacto = dto.Contacto,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Direccion = dto.Direccion,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        await _repo.CreateClienteAsync(cliente);
        return (true, "Cliente registrado correctamente.", cliente);
    }

    public async Task<(bool Success, string Message)> UpdateClienteAsync(int id, ClienteRequestDto dto)
    {
        var cliente = await _repo.GetClienteByIdAsync(id);
        if (cliente is null) return (false, "Cliente no encontrado.");

        if (cliente.RFC != dto.RFC && await _repo.ExisteClienteRFCAsync(dto.RFC))
            return (false, "Ya existe un cliente con ese RFC.");

        cliente.Nombre = dto.Nombre;
        cliente.RFC = dto.RFC;
        cliente.Contacto = dto.Contacto;
        cliente.Telefono = dto.Telefono;
        cliente.Email = dto.Email;
        cliente.Direccion = dto.Direccion;

        await _repo.UpdateClienteAsync(cliente);
        return (true, "Cliente actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteClienteAsync(int id)
    {
        var cliente = await _repo.GetClienteByIdAsync(id);
        if (cliente is null) return (false, "Cliente no encontrado.");

        await _repo.DeleteClienteAsync(cliente);
        return (true, "Cliente desactivado correctamente.");
    }
}
