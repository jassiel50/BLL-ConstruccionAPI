using System.Security.Claims;
using BLL_ConstruccionAPI.DTOs.ProveedoresClientes;
using BLL_ConstruccionAPI.Models.Inventario;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BLL_ConstruccionAPI.Services;

public class ProveedoresClientesService : IProveedoresClientesService
{
    private readonly IProveedoresClientesRepository _repo;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProveedoresClientesService(
        IProveedoresClientesRepository repo,
        IBitacoraService bitacora,
        IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
    }

    private (int Id, string Nombre, string Ip) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "";
        return (id, nombre, ip);
    }

    // ─── Proveedores ──────────────────────────────────────────────────────────
    public async Task<IEnumerable<ProveedorResponseDto>> GetAllProveedoresAsync()
    {
        var proveedores = await _repo.GetAllProveedoresAsync();
        return proveedores.Select(ProveedorResponseDto.FromEntity);
    }

    public async Task<ProveedorResponseDto?> GetProveedorByIdAsync(int id)
    {
        var proveedor = await _repo.GetProveedorByIdAsync(id);
        return proveedor is null ? null : ProveedorResponseDto.FromEntity(proveedor);
    }

    public async Task<(bool Success, string Message, ProveedorResponseDto? Data)> CreateProveedorAsync(ProveedorRequestDto dto)
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

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó proveedor", "Proveedor",
            $"Proveedor '{proveedor.Nombre}' (RFC: {proveedor.RFC}) registrado.", ip);

        return (true, "Proveedor registrado correctamente.", ProveedorResponseDto.FromEntity(proveedor));
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

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Actualizó proveedor", "Proveedor",
            $"Proveedor '{proveedor.Nombre}' (RFC: {proveedor.RFC}) actualizado.", ip);

        return (true, "Proveedor actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteProveedorAsync(int id)
    {
        var proveedor = await _repo.GetProveedorByIdAsync(id);
        if (proveedor is null) return (false, "Proveedor no encontrado.");

        await _repo.DeleteProveedorAsync(proveedor);

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Desactivó proveedor", "Proveedor",
            $"Proveedor '{proveedor.Nombre}' (RFC: {proveedor.RFC}) desactivado.", ip);

        return (true, "Proveedor desactivado correctamente.");
    }

    // ─── Clientes ─────────────────────────────────────────────────────────────
    public async Task<IEnumerable<ClienteResponseDto>> GetAllClientesAsync()
    {
        var clientes = await _repo.GetAllClientesAsync();
        return clientes.Select(ClienteResponseDto.FromEntity);
    }

    public async Task<ClienteResponseDto?> GetClienteByIdAsync(int id)
    {
        var cliente = await _repo.GetClienteByIdAsync(id);
        return cliente is null ? null : ClienteResponseDto.FromEntity(cliente);
    }

    public async Task<(bool Success, string Message, ClienteResponseDto? Data)> CreateClienteAsync(ClienteRequestDto dto)
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

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó cliente", "Cliente",
            $"Cliente '{cliente.Nombre}' (RFC: {cliente.RFC}) registrado.", ip);

        return (true, "Cliente registrado correctamente.", ClienteResponseDto.FromEntity(cliente));
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

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Actualizó cliente", "Cliente",
            $"Cliente '{cliente.Nombre}' (RFC: {cliente.RFC}) actualizado.", ip);

        return (true, "Cliente actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteClienteAsync(int id)
    {
        var cliente = await _repo.GetClienteByIdAsync(id);
        if (cliente is null) return (false, "Cliente no encontrado.");

        await _repo.DeleteClienteAsync(cliente);

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Desactivó cliente", "Cliente",
            $"Cliente '{cliente.Nombre}' (RFC: {cliente.RFC}) desactivado.", ip);

        return (true, "Cliente desactivado correctamente.");
    }
}
