using BLL_ConstruccionAPI.DTOs.Fases;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
public class FaseController : ControllerBase
{
    private readonly IFasesService _service;

    public FaseController(IFasesService service)
    {
        _service = service;
    }

    // GET api/proyectos/{proyectoId}/fases
    [HttpGet("api/proyectos/{proyectoId:int}/fases")]
    public async Task<IActionResult> GetByProyecto(int proyectoId)
    {
        var fases = await _service.GetByProyectoAsync(proyectoId);
        return Ok(fases);
    }

    // POST api/proyectos/{proyectoId}/fases
    [HttpPost("api/proyectos/{proyectoId:int}/fases")]
    public async Task<IActionResult> Create(int proyectoId, [FromBody] FaseRequestDto dto)
    {
        var (success, message, data) = await _service.CreateAsync(proyectoId, dto);
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // PUT api/fases/{id}
    [HttpPut("api/fases/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] FaseRequestDto dto)
    {
        var (success, message) = await _service.UpdateAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // PUT api/fases/{id}/descompletar
    [HttpPut("api/fases/{id:int}/descompletar")]
    public async Task<IActionResult> Descompletar(int id)
    {
        var (success, message) = await _service.DescompletarAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // PUT api/fases/{id}/completar
    [HttpPut("api/fases/{id:int}/completar")]
    public async Task<IActionResult> Completar(int id)
    {
        var (success, message) = await _service.CompletarAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/fases/{id}
    [HttpDelete("api/fases/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // GET api/fases/atrasadas
    [HttpGet("api/fases/atrasadas")]
    public async Task<IActionResult> GetAtrasadas()
    {
        var fases = await _service.GetAtrasadasAsync();
        return Ok(fases);
    }

    // GET api/fases/por-vencer
    [HttpGet("api/fases/por-vencer")]
    public async Task<IActionResult> GetPorVencer()
    {
        var fases = await _service.GetPorVencerAsync();
        return Ok(fases);
    }
}
