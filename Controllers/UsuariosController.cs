using BLL_ConstruccionAPI.DTOs.Auth;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuariosService _usuariosService;

    public UsuariosController(IUsuariosService usuariosService)
    {
        _usuariosService = usuariosService;
    }

    // GET api/usuarios
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rolId = User.FindFirstValue("rolId");
        if (rolId != "1" && rolId != "3") return Forbid();
        var result = await _usuariosService.GetAllAsync();
        return Ok(result);
    }

    // GET api/usuarios/directorio
    // Lista mínima (id, nombre, email) de usuarios activos, sin restricción de rol —
    // usada por selectores como el de destinatarios de reportes programados.
    [HttpGet("directorio")]
    public async Task<IActionResult> GetDirectorio()
    {
        var result = await _usuariosService.GetDirectorioAsync();
        return Ok(result);
    }

    // GET api/usuarios/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rolId = User.FindFirstValue("rolId");
        if (rolId != "1" && rolId != "3") return Forbid();
        var usuario = await _usuariosService.GetByIdAsync(id);
        if (usuario is null) return NotFound(new { message = "Usuario no encontrado." });
        return Ok(usuario);
    }

    // POST api/usuarios
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] RegisterRequestDto dto)
    {
        var rolId = User.FindFirstValue("rolId");
        if (rolId != "1" && rolId != "3") return Forbid();
        var (success, message) = await _usuariosService.CrearAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // PUT api/usuarios/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] UsuarioUpdateDto dto)
    {
        var rolId = User.FindFirstValue("rolId");
        if (rolId != "1" && rolId != "3") return Forbid();
        var (success, message) = await _usuariosService.ActualizarAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // PUT api/usuarios/{id}/toggle-activo
    [HttpPut("{id:int}/toggle-activo")]
    public async Task<IActionResult> ToggleActivo(int id)
    {
        var rolId = User.FindFirstValue("rolId");
        if (rolId != "1" && rolId != "3") return Forbid();
        var (success, message) = await _usuariosService.ToggleActivoAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
