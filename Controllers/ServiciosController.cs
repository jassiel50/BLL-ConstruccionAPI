using BLL_ConstruccionAPI.DTOs.Servicios;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/servicios")]
public class ServiciosController : ControllerBase
{
    private readonly IServiciosService _service;

    public ServiciosController(IServiciosService service)
    {
        _service = service;
    }

    // GET api/servicios
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var (usuarioId, rolId) = GetUsuario();
        var servicios = await _service.GetAllAsync(rolId, usuarioId);
        return Ok(servicios);
    }

    // GET api/servicios/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var (usuarioId, rolId) = GetUsuario();
        var servicio = await _service.GetByIdAsync(id, rolId, usuarioId);
        if (servicio is null) return NotFound(new { message = "Servicio no encontrado." });
        return Ok(servicio);
    }

    // POST api/servicios
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ServicioRequestDto dto)
    {
        var (usuarioId, _) = GetUsuario();
        var nombreUsuario = User.FindFirstValue("nombreUsuario") ?? "Operador";

        var (success, message, data) = await _service.CreateAsync(usuarioId, nombreUsuario, dto);
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // PUT api/servicios/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ServicioRequestDto dto)
    {
        var (usuarioId, _) = GetUsuario();
        var (success, message, data) = await _service.UpdateAsync(id, usuarioId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // POST api/servicios/{id}/firmar
    [HttpPost("{id:int}/firmar")]
    public async Task<IActionResult> Firmar(int id, [FromBody] ServicioFirmarDto dto)
    {
        var (usuarioId, _) = GetUsuario();
        var (success, message, data) = await _service.FirmarAsync(id, usuarioId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // DELETE api/servicios/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (usuarioId, _) = GetUsuario();
        var (success, message) = await _service.DeleteAsync(id, usuarioId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    private (int UsuarioId, int RolId) GetUsuario()
    {
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId);
        int.TryParse(User.FindFirstValue("rolId"), out var rolId);
        return (usuarioId, rolId);
    }
}
