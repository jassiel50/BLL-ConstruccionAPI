using BLL_ConstruccionAPI.DTOs.Perdidas;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/perdidas")]
public class PerdidasController : ControllerBase
{
    private readonly IPerdidasService _service;

    public PerdidasController(IPerdidasService service)
    {
        _service = service;
    }

    // GET api/perdidas
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var registros = await _service.GetAllAsync();
        return Ok(registros);
    }

    // GET api/perdidas/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var registro = await _service.GetByIdAsync(id);
        if (registro is null) return NotFound(new { message = "Registro de pérdida no encontrado." });
        return Ok(registro);
    }

    // GET api/perdidas/proyecto/{proyectoId}
    [HttpGet("proyecto/{proyectoId:int}")]
    public async Task<IActionResult> GetByProyecto(int proyectoId)
    {
        var registros = await _service.GetByProyectoAsync(proyectoId);
        return Ok(registros);
    }

    // GET api/perdidas/material/{materialId}
    [HttpGet("material/{materialId:int}")]
    public async Task<IActionResult> GetByMaterial(int materialId)
    {
        var registros = await _service.GetByMaterialAsync(materialId);
        return Ok(registros);
    }

    // GET api/perdidas/herramienta/{herramientaId}
    [HttpGet("herramienta/{herramientaId:int}")]
    public async Task<IActionResult> GetByHerramienta(int herramientaId)
    {
        var registros = await _service.GetByHerramientaAsync(herramientaId);
        return Ok(registros);
    }

    // POST api/perdidas
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RegistroPerdidaRequestDto dto)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized(new { message = "Token inválido." });

        var (success, message, data) = await _service.CreateAsync(usuarioId, dto);
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // DELETE api/perdidas/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
