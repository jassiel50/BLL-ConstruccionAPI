using BLL_ConstruccionAPI.DTOs.Reportes;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/reportes/configuraciones")]
public class ConfiguracionReporteController : ControllerBase
{
    private readonly IConfiguracionReporteService _service;

    public ConfiguracionReporteController(IConfiguracionReporteService service)
    {
        _service = service;
    }

    // GET api/reportes/configuraciones
    [HttpGet]
    public async Task<IActionResult> GetMias()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized();

        var configs = await _service.GetMisConfiguracionesAsync(usuarioId);
        return Ok(configs);
    }

    // GET api/reportes/configuraciones/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized();

        var config = await _service.GetByIdAsync(id, usuarioId);
        if (config is null) return NotFound(new { message = "Configuración no encontrada." });
        return Ok(config);
    }

    // POST api/reportes/configuraciones
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ConfiguracionReporteRequestDto dto)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized();

        var (success, message, data) = await _service.CreateAsync(dto, usuarioId);
        if (!success) return BadRequest(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { message, data });
    }

    // PUT api/reportes/configuraciones/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ConfiguracionReporteRequestDto dto)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized();

        var (success, message) = await _service.UpdateAsync(id, dto, usuarioId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/reportes/configuraciones/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized();

        var (success, message) = await _service.DeleteAsync(id, usuarioId);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }

    // POST api/reportes/configuraciones/{id}/enviar-ahora
    [HttpPost("{id:int}/enviar-ahora")]
    public async Task<IActionResult> EnviarAhora(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized();

        var (success, message) = await _service.EnviarAhoraAsync(id, usuarioId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
