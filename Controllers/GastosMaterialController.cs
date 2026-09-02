using BLL_ConstruccionAPI.DTOs.Proyectos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
public class GastosMaterialController : ControllerBase
{
    private readonly IGastoMaterialService _service;

    public GastosMaterialController(IGastoMaterialService service)
    {
        _service = service;
    }

    // GET api/proyectos/{id}/gastos-material
    [HttpGet("api/proyectos/{id:int}/gastos-material")]
    public async Task<IActionResult> GetByProyecto(int id)
        => Ok(await _service.GetByProyectoAsync(id));

    // POST api/proyectos/{id}/gastos-material
    [HttpPost("api/proyectos/{id:int}/gastos-material")]
    public async Task<IActionResult> Create(int id, [FromBody] GastoMaterialRequestDto dto)
    {
        var (success, message, data) = await _service.CreateAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Created(string.Empty, new { message, data });
    }

    // DELETE api/gastos-material/{id}
    [HttpDelete("api/gastos-material/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }
}
