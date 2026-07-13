using BLL_ConstruccionAPI.DTOs.Auth;
using BLL_ConstruccionAPI.Models.Auth;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services.Interfaces;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Services;

public class UsuariosService : IUsuariosService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IBitacoraService _bitacora;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuariosService(
        IUsuarioRepository usuarioRepo,
        IBitacoraService bitacora,
        IHttpContextAccessor httpContextAccessor)
    {
        _usuarioRepo = usuarioRepo;
        _bitacora = bitacora;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<UsuarioResponseDto>> GetAllAsync()
    {
        var usuarios = await _usuarioRepo.GetAllAsync();
        return usuarios.Select(u => new UsuarioResponseDto
        {
            Id             = u.Id,
            NombreUsuario  = u.NombreUsuario,
            Nombre         = u.Nombre,
            Email          = u.Email,
            EmailSecundario = u.EmailSecundario,
            RolId          = u.RolId,
            NombreRol      = u.Rol?.Nombre ?? string.Empty,
            Activo         = u.Activo,
            FechaCreacion  = u.FechaCreacion,
            UltimoAcceso   = u.UltimoAcceso
        });
    }

    public async Task<IEnumerable<UsuarioDirectorioDto>> GetDirectorioAsync()
    {
        var usuarios = await _usuarioRepo.GetAllAsync();
        return usuarios
            .Where(u => u.Activo)
            .OrderBy(u => u.Nombre)
            .Select(u => new UsuarioDirectorioDto { Id = u.Id, Nombre = u.Nombre, Email = u.Email, EmailSecundario = u.EmailSecundario });
    }

    public async Task<UsuarioResponseDto?> GetByIdAsync(int id)
    {
        var u = await _usuarioRepo.GetByIdConRolAsync(id);
        if (u is null) return null;

        return new UsuarioResponseDto
        {
            Id             = u.Id,
            NombreUsuario  = u.NombreUsuario,
            Nombre         = u.Nombre,
            Email          = u.Email,
            EmailSecundario = u.EmailSecundario,
            RolId          = u.RolId,
            NombreRol      = u.Rol?.Nombre ?? string.Empty,
            Activo         = u.Activo,
            FechaCreacion  = u.FechaCreacion,
            UltimoAcceso   = u.UltimoAcceso
        };
    }

    public async Task<(bool Success, string Message)> CrearAsync(RegisterRequestDto dto)
    {
        if (await _usuarioRepo.ExisteNombreUsuarioAsync(dto.NombreUsuario))
            return (false, "El nombre de usuario ya existe.");

        if (await _usuarioRepo.ExisteEmailAsync(dto.Email))
            return (false, "El email ya está registrado.");

        var usuario = new Usuario
        {
            NombreUsuario   = dto.NombreUsuario,
            Nombre          = dto.Nombre,
            Email           = dto.Email,
            EmailSecundario = string.IsNullOrWhiteSpace(dto.EmailSecundario) ? null : dto.EmailSecundario.Trim(),
            PasswordHash    = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RolId           = dto.RolId,
            Activo          = true,
            FechaCreacion   = DateTime.UtcNow
        };

        await _usuarioRepo.CreateAsync(usuario);

        var rolNombre = dto.RolId switch
        {
            1 => "Admin",
            3 => "Sistemas",
            _ => "Operador"
        };
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Creó", "Usuario", $"Usuario '{dto.NombreUsuario}' creado con rol {rolNombre}");

        return (true, "Usuario registrado correctamente.");
    }

    public async Task<(bool Success, string Message)> ActualizarAsync(int id, UsuarioUpdateDto dto)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(id);
        if (usuario is null) return (false, "Usuario no encontrado.");

        var existente = await _usuarioRepo.GetByEmailAsync(dto.Email);
        if (existente is not null && existente.Id != id)
            return (false, "El email ya está registrado por otro usuario.");

        usuario.Nombre = dto.Nombre.Trim();
        usuario.Email = dto.Email.Trim();
        usuario.EmailSecundario = string.IsNullOrWhiteSpace(dto.EmailSecundario) ? null : dto.EmailSecundario.Trim();
        usuario.RolId = dto.RolId;

        await _usuarioRepo.UpdateAsync(usuario);

        var rolNombre = dto.RolId switch
        {
            1 => "Admin",
            2 => "Operador",
            3 => "Sistemas",
            _ => $"Rol {dto.RolId}"
        };
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Actualizó", "Usuario", $"Usuario '{usuario.NombreUsuario}' editado (rol: {rolNombre}, email: {usuario.Email})");

        return (true, "Usuario actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> ToggleActivoAsync(int id)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(id);
        if (usuario is null) return (false, "Usuario no encontrado.");

        usuario.Activo = !usuario.Activo;
        await _usuarioRepo.UpdateAsync(usuario);

        var estado = usuario.Activo ? "activado" : "desactivado";
        var (uid, uname) = GetUsuarioInfo();
        await _bitacora.RegistrarAsync(uid, uname, "Actualizó", "Usuario", $"Usuario '{usuario.NombreUsuario}' {estado}");

        return (true, $"Usuario {estado} correctamente.");
    }

    private (int UsuarioId, string NombreUsuario) GetUsuarioInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        var nombre = user?.FindFirstValue("nombreUsuario") ?? "Sistema";
        return (id, nombre);
    }
}
