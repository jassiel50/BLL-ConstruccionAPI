using BLL_ConstruccionAPI.DTOs.Checklist;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
public class ChecklistController : ControllerBase
{
    private readonly IChecklistService _service;

    public ChecklistController(IChecklistService service)
    {
        _service = service;
    }

    // GET api/proyectos/{proyectoId}/checklist
    [HttpGet("api/proyectos/{proyectoId:int}/checklist")]
    public async Task<IActionResult> GetByProyecto(int proyectoId)
        => Ok(await _service.GetByProyectoAsync(proyectoId));

    // GET api/fases/{faseId}/checklist
    [HttpGet("api/fases/{faseId:int}/checklist")]
    public async Task<IActionResult> GetByFase(int faseId)
        => Ok(await _service.GetByFaseAsync(faseId));

    // POST api/proyectos/{proyectoId}/checklist
    [HttpPost("api/proyectos/{proyectoId:int}/checklist")]
    public async Task<IActionResult> Create(int proyectoId, [FromBody] ChecklistItemRequestDto dto)
    {
        var (success, message, data) = await _service.CreateAsync(proyectoId, dto);
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // PUT api/checklist/{id}/completar
    [HttpPut("api/checklist/{id:int}/completar")]
    public async Task<IActionResult> Completar(int id)
    {
        var (success, message) = await _service.CompletarAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // PUT api/checklist/{id}/descompletar
    [HttpPut("api/checklist/{id:int}/descompletar")]
    public async Task<IActionResult> Descompletar(int id)
    {
        var (success, message) = await _service.DescompletarAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/checklist/{id}
    [HttpDelete("api/checklist/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return NotFound(new { message });
        return NoContent();
    }
}
