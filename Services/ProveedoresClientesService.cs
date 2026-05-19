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

    private static ContactoResponseDto MapContacto(ContactoCliente c) => new()
    {
        Id = c.Id,
        Nombre = c.Nombre,
        Telefono = c.Telefono,
        Email = c.Email,
        Cargo = c.Cargo,
        EsPrincipal = c.EsPrincipal
    };

    private static ContactoResponseDto MapContacto(ContactoProveedor c) => new()
    {
        Id = c.Id,
        Nombre = c.Nombre,
        Telefono = c.Telefono,
        Email = c.Email,
        Cargo = c.Cargo,
        EsPrincipal = c.EsPrincipal
    };

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
            Direccion = dto.Direccion,
            Estado = dto.Estado,
            Descripcion = dto.Descripcion,
            CategoriaId = dto.CategoriaId,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        await _repo.CreateProveedorAsync(proveedor);

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó proveedor", "Proveedor",
            $"Proveedor '{proveedor.Nombre}' (RFC: {proveedor.RFC}) registrado.", ip);

        var created = await _repo.GetProveedorByIdAsync(proveedor.Id);
        return (true, "Proveedor registrado correctamente.", ProveedorResponseDto.FromEntity(created!));
    }

    public async Task<(bool Success, string Message)> UpdateProveedorAsync(int id, ProveedorRequestDto dto)
    {
        var proveedor = await _repo.GetProveedorByIdAsync(id);
        if (proveedor is null) return (false, "Proveedor no encontrado.");

        if (proveedor.RFC != dto.RFC && await _repo.ExisteProveedorRFCAsync(dto.RFC))
            return (false, "Ya existe un proveedor con ese RFC.");

        proveedor.Nombre = dto.Nombre;
        proveedor.RFC = dto.RFC;
        proveedor.Direccion = dto.Direccion;
        proveedor.Estado = dto.Estado;
        proveedor.Descripcion = dto.Descripcion;
        proveedor.CategoriaId = dto.CategoriaId;

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

    // ─── Contactos Proveedor ──────────────────────────────────────────────────
    public async Task<(bool Success, string Message, ContactoResponseDto? Data)> AddContactoProveedorAsync(int proveedorId, ContactoRequestDto dto)
    {
        var proveedor = await _repo.GetProveedorByIdAsync(proveedorId);
        if (proveedor is null) return (false, "Proveedor no encontrado.", null);

        var contacto = new ContactoProveedor
        {
            ProveedorId = proveedorId,
            Nombre = dto.Nombre,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Cargo = dto.Cargo,
            EsPrincipal = dto.EsPrincipal
        };

        await _repo.AddContactoProveedorAsync(contacto);

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Agregó contacto a proveedor", "Proveedor",
            $"Contacto '{contacto.Nombre}' agregado al proveedor '{proveedor.Nombre}'.", ip);

        return (true, "Contacto agregado correctamente.", MapContacto(contacto));
    }

    public async Task<(bool Success, string Message)> UpdateContactoProveedorAsync(int proveedorId, int contactoId, ContactoRequestDto dto)
    {
        var contacto = await _repo.GetContactoProveedorAsync(contactoId);
        if (contacto is null || contacto.ProveedorId != proveedorId)
            return (false, "Contacto no encontrado.");

        contacto.Nombre = dto.Nombre;
        contacto.Telefono = dto.Telefono;
        contacto.Email = dto.Email;
        contacto.Cargo = dto.Cargo;
        contacto.EsPrincipal = dto.EsPrincipal;

        await _repo.UpdateContactoProveedorAsync(contacto);
        return (true, "Contacto actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteContactoProveedorAsync(int proveedorId, int contactoId)
    {
        var contacto = await _repo.GetContactoProveedorAsync(contactoId);
        if (contacto is null || contacto.ProveedorId != proveedorId)
            return (false, "Contacto no encontrado.");

        await _repo.DeleteContactoProveedorAsync(contacto);
        return (true, "Contacto eliminado correctamente.");
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
            Direccion = dto.Direccion,
            Estado = dto.Estado,
            Descripcion = dto.Descripcion,
            CategoriaId = dto.CategoriaId,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        await _repo.CreateClienteAsync(cliente);

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó cliente", "Cliente",
            $"Cliente '{cliente.Nombre}' (RFC: {cliente.RFC}) registrado.", ip);

        var created = await _repo.GetClienteByIdAsync(cliente.Id);
        return (true, "Cliente registrado correctamente.", ClienteResponseDto.FromEntity(created!));
    }

    public async Task<(bool Success, string Message)> UpdateClienteAsync(int id, ClienteRequestDto dto)
    {
        var cliente = await _repo.GetClienteByIdAsync(id);
        if (cliente is null) return (false, "Cliente no encontrado.");

        if (cliente.RFC != dto.RFC && await _repo.ExisteClienteRFCAsync(dto.RFC))
            return (false, "Ya existe un cliente con ese RFC.");

        cliente.Nombre = dto.Nombre;
        cliente.RFC = dto.RFC;
        cliente.Direccion = dto.Direccion;
        cliente.Estado = dto.Estado;
        cliente.Descripcion = dto.Descripcion;
        cliente.CategoriaId = dto.CategoriaId;

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

    // ─── Contactos Cliente ────────────────────────────────────────────────────
    public async Task<(bool Success, string Message, ContactoResponseDto? Data)> AddContactoClienteAsync(int clienteId, ContactoRequestDto dto)
    {
        var cliente = await _repo.GetClienteByIdAsync(clienteId);
        if (cliente is null) return (false, "Cliente no encontrado.", null);

        var contacto = new ContactoCliente
        {
            ClienteId = clienteId,
            Nombre = dto.Nombre,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Cargo = dto.Cargo,
            EsPrincipal = dto.EsPrincipal
        };

        await _repo.AddContactoClienteAsync(contacto);

        var (uid, uname, ip) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Agregó contacto a cliente", "Cliente",
            $"Contacto '{contacto.Nombre}' agregado al cliente '{cliente.Nombre}'.", ip);

        return (true, "Contacto agregado correctamente.", MapContacto(contacto));
    }

    public async Task<(bool Success, string Message)> UpdateContactoClienteAsync(int clienteId, int contactoId, ContactoRequestDto dto)
    {
        var contacto = await _repo.GetContactoClienteAsync(contactoId);
        if (contacto is null || contacto.ClienteId != clienteId)
            return (false, "Contacto no encontrado.");

        contacto.Nombre = dto.Nombre;
        contacto.Telefono = dto.Telefono;
        contacto.Email = dto.Email;
        contacto.Cargo = dto.Cargo;
        contacto.EsPrincipal = dto.EsPrincipal;

        await _repo.UpdateContactoClienteAsync(contacto);
        return (true, "Contacto actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteContactoClienteAsync(int clienteId, int contactoId)
    {
        var contacto = await _repo.GetContactoClienteAsync(contactoId);
        if (contacto is null || contacto.ClienteId != clienteId)
            return (false, "Contacto no encontrado.");

        await _repo.DeleteContactoClienteAsync(contacto);
        return (true, "Contacto eliminado correctamente.");
    }
}
