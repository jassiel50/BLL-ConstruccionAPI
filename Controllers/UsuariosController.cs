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
        if (User.FindFirstValue("rolId") != "1") return Forbid();
        var result = await _usuariosService.GetAllAsync();
        return Ok(result);
    }

    // GET api/usuarios/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (User.FindFirstValue("rolId") != "1") return Forbid();
        var usuario = await _usuariosService.GetByIdAsync(id);
        if (usuario is null) return NotFound(new { message = "Usuario no encontrado." });
        return Ok(usuario);
    }

    // POST api/usuarios
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] RegisterRequestDto dto)
    {
        if (User.FindFirstValue("rolId") != "1") return Forbid();
        var (success, message) = await _usuariosService.CrearAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // PUT api/usuarios/{id}/toggle-activo
    [HttpPut("{id:int}/toggle-activo")]
    public async Task<IActionResult> ToggleActivo(int id)
    {
        if (User.FindFirstValue("rolId") != "1") return Forbid();
        var (success, message) = await _usuariosService.ToggleActivoAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
