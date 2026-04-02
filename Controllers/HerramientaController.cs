using BLL_ConstruccionAPI.DTOs.Herramientas;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[ApiController]
[Route("api/herramientas")]
public class HerramientaController : ControllerBase
{
    private readonly IHerramientasService _service;

    public HerramientaController(IHerramientasService service)
    {
        _service = service;
    }

    // GET api/herramientas
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var herramientas = await _service.GetAllAsync();
        return Ok(herramientas);
    }

    // GET api/herramientas/disponibles
    [HttpGet("disponibles")]
    public async Task<IActionResult> GetDisponibles()
    {
        var herramientas = await _service.GetDisponiblesAsync();
        return Ok(herramientas);
    }

    // GET api/herramientas/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var herramienta = await _service.GetByIdAsync(id);
        if (herramienta is null) return NotFound(new { message = "Herramienta no encontrada." });
        return Ok(herramienta);
    }

    // GET api/herramientas/{id}/asignaciones
    [HttpGet("{id:int}/asignaciones")]
    public async Task<IActionResult> GetAsignaciones(int id)
    {
        var asignaciones = await _service.GetAsignacionesAsync(id);
        return Ok(asignaciones);
    }

    // POST api/herramientas
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] HerramientaRequestDto dto)
    {
        var (success, message, data) = await _service.CreateAsync(dto);
        if (!success) return BadRequest(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { message, data });
    }

    // PUT api/herramientas/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] HerramientaRequestDto dto)
    {
        var (success, message) = await _service.UpdateAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/herramientas/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // POST api/herramientas/asignar
    [HttpPost("asignar")]
    public async Task<IActionResult> Asignar([FromBody] AsignacionRequestDto dto)
    {
        var (success, message, data) = await _service.AsignarAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // PUT api/herramientas/devolver/{asignacionId}
    [HttpPut("devolver/{asignacionId:int}")]
    public async Task<IActionResult> Devolver(int asignacionId, [FromBody] DevolucionRequestDto dto)
    {
        var (success, message) = await _service.DevolverAsync(asignacionId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
